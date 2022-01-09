// <copyright file="MpqStream.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Tools.Mpq;

public class MpqStream : Stream
{
    private readonly Stream stream;
    private readonly int blockSize;

    private readonly MpqEntry entry;
    private uint[]? blockPositions;

    private long position;
    private byte[]? currentData;
    private int currentBlockIndex = -1;

    internal MpqStream(MpqArchive archive, MpqEntry entry)
    {
        this.entry = entry;

        this.stream = archive.BaseStream;
        this.blockSize = archive.BlockSize;

        if (this.entry.IsCompressed && !this.entry.IsSingleUnit)
        {
            this.LoadBlockPositions();
        }
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => this.entry.FileSize;

    public override long Position
    {
        get
        {
            return this.position;
        }

        set
        {
            this.Seek(value, SeekOrigin.Begin);
        }
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        var target = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => this.Position + offset,
            SeekOrigin.End => this.Length + offset,
            _ => throw new ArgumentException("Origin", nameof(origin)),
        };

        if (target < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(target), "Attmpted to Seek before the beginning of the stream");
        }

        if (target >= this.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(target), "Attmpted to Seek beyond the end of the stream");
        }

        this.position = target;

        return this.position;
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException("SetLength is not supported");
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (this.entry.IsSingleUnit)
        {
            return this.ReadInternalSingleUnit(buffer, offset, count);
        }

        int toread = count;
        int readtotal = 0;

        while (toread > 0)
        {
            int read = this.ReadInternal(buffer, offset, toread);
            if (read == 0)
            {
                break;
            }

            readtotal += read;
            offset += read;
            toread -= read;
        }

        return readtotal;
    }

    public override int ReadByte()
    {
        if (this.position >= this.Length)
        {
            return -1;
        }

        if (this.entry.IsSingleUnit)
        {
            return this.ReadByteSingleUnit();
        }

        this.BufferData();

        int localposition = (int)(this.position % this.blockSize);
        this.position++;
        return this.currentData[localposition];
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("Writing is not supported");
    }

    private static byte[] DecompressMulti(byte[] input, int outputLength)
    {
        Stream sinput = new MemoryStream(input);

        byte comptype = (byte)sinput.ReadByte();

        // WC3 onward mosly use Zlib
        // Starcraft 1 mostly uses PKLib, plus types 41 and 81 for audio files
        switch (comptype)
        {
            case 1: // Huffman
                return MpqHuffman.Decompress(sinput).ToArray();
            case 2: // ZLib/Deflate
                return ZlibDecompress(sinput, outputLength);
            case 8: // PKLib/Impode
                return PKDecompress(sinput, outputLength);
            case 0x10: // BZip2
                return BZip2Decompress(sinput, outputLength);
            case 0x80: // IMA ADPCM Stereo
                return MpqWavCompression.Decompress(sinput, 2);
            case 0x40: // IMA ADPCM Mono
                return MpqWavCompression.Decompress(sinput, 1);
            case 0x12:
                // TODO: LZMA
                throw new MpqParserException("LZMA compression is not yet supported");

            // Combos
            case 0x22:
                // TODO: sparse then zlib
                throw new MpqParserException("Sparse compression + Deflate compression is not yet supported");
            case 0x30:
                // TODO: sparse then bzip2
                throw new MpqParserException("Sparse compression + BZip2 compression is not yet supported");
            case 0x41:
                sinput = MpqHuffman.Decompress(sinput);
                return MpqWavCompression.Decompress(sinput, 1);
            case 0x48:
                byte[] result = PKDecompress(sinput, outputLength);
                return MpqWavCompression.Decompress(new MemoryStream(result), 1);
            case 0x81:
                sinput = MpqHuffman.Decompress(sinput);
                return MpqWavCompression.Decompress(sinput, 2);
            case 0x88:
                byte[] res = PKDecompress(sinput, outputLength);
                return MpqWavCompression.Decompress(new MemoryStream(res), 2);
            default:
                throw new MpqParserException("Compression is not yet supported: 0x" + comptype.ToString("X"));
        }
    }

    private static byte[] BZip2Decompress(Stream data, int expectedLength)
    {
        using MemoryStream output = new (expectedLength);
        using (var stream = new Ionic.BZip2.BZip2InputStream(data, false))
        {
            stream.CopyTo(output);
        }

        return output.ToArray();
    }

    private static byte[] PKDecompress(Stream data, int expectedLength)
    {
        PKLibDecompress pk = new (data);
        return pk.Explode(expectedLength);
    }

    private static byte[] ZlibDecompress(Stream data, int expectedLength)
    {
        using MemoryStream output = new (expectedLength);
        using (var stream = new Ionic.Zlib.ZlibStream(data, Ionic.Zlib.CompressionMode.Decompress))
        {
            stream.CopyTo(output);
        }

        return output.ToArray();
    }

    // Compressed files start with an array of offsets to make seeking possible
    private void LoadBlockPositions()
    {
        int blockposcount = (int)((this.entry.FileSize + this.blockSize - 1) / this.blockSize) + 1;

        // Files with metadata have an extra block containing block checksums
        if ((this.entry.Flags & MpqFileFlags.FileHasMetadata) != 0)
        {
            blockposcount++;
        }

        this.blockPositions = new uint[blockposcount];

        lock (this.stream)
        {
            this.stream.Seek(this.entry.FilePos, SeekOrigin.Begin);
            BinaryReader br = new (this.stream);

            for (int i = 0; i < blockposcount; i++)
            {
                this.blockPositions[i] = br.ReadUInt32();
            }
        }

        uint blockpossize = (uint)blockposcount * 4;

        /*
        if(_blockPositions[0] != blockpossize)
            _entry.Flags |= MpqFileFlags.Encrypted;
         */

        if (this.entry.IsEncrypted)
        {
            // This should only happen when the file name is not known
            if (this.entry.EncryptionSeed == 0)
            {
                this.entry.EncryptionSeed = MpqArchive.DetectFileSeed(this.blockPositions[0], this.blockPositions[1], blockpossize) + 1;
                if (this.entry.EncryptionSeed == 1)
                {
                    throw new MpqParserException("Unable to determine encyption seed");
                }
            }

            MpqArchive.DecryptBlock(this.blockPositions, this.entry.EncryptionSeed - 1);

            if (this.blockPositions[0] != blockpossize)
            {
                throw new MpqParserException("Decryption failed");
            }

            if (this.blockPositions[1] > this.blockSize + blockpossize)
            {
                throw new MpqParserException("Decryption failed");
            }
        }
    }

    private byte[] LoadBlock(int blockIndex, int expectedLength)
    {
        uint offset;
        int toread;
        uint encryptionseed;

        if (this.entry.IsCompressed)
        {
            offset = this.blockPositions[blockIndex];
            toread = (int)(this.blockPositions[blockIndex + 1] - offset);
        }
        else
        {
            offset = (uint)(blockIndex * this.blockSize);
            toread = expectedLength;
        }

        offset += this.entry.FilePos;

        byte[] data = new byte[toread];
        lock (this.stream)
        {
            this.stream.Seek(offset, SeekOrigin.Begin);
            int read = this.stream.Read(data, 0, toread);
            if (read != toread)
            {
                throw new MpqParserException("Insufficient data or invalid data length");
            }
        }

        if (this.entry.IsEncrypted && this.entry.FileSize > 3)
        {
            if (this.entry.EncryptionSeed == 0)
            {
                throw new MpqParserException("Unable to determine encryption key");
            }

            encryptionseed = (uint)(blockIndex + this.entry.EncryptionSeed);
            MpqArchive.DecryptBlock(data, encryptionseed);
        }

        if (this.entry.IsCompressed && (toread != expectedLength))
        {
            if ((this.entry.Flags & MpqFileFlags.CompressedMulti) != 0)
            {
                data = DecompressMulti(data, expectedLength);
            }
            else
            {
                data = PKDecompress(new MemoryStream(data), expectedLength);
            }
        }

        return data;
    }

    // SingleUnit entries can be compressed but are never encrypted
    private int ReadInternalSingleUnit(byte[] buffer, int offset, int count)
    {
        if (this.position >= this.Length)
        {
            return 0;
        }

        if (this.currentData is null)
        {
            this.LoadSingleUnit();
        }

        int bytestocopy = Math.Min((int)(this.currentData.Length - this.position), count);

        Array.Copy(this.currentData, this.position, buffer, offset, bytestocopy);

        this.position += bytestocopy;
        return bytestocopy;
    }

    private void LoadSingleUnit()
    {
        // Read the entire file into memory
        byte[] filedata = new byte[this.entry.CompressedSize];
        lock (this.stream)
        {
            this.stream.Seek(this.entry.FilePos, SeekOrigin.Begin);
            int read = this.stream.Read(filedata, 0, filedata.Length);
            if (read != filedata.Length)
            {
                throw new MpqParserException("Insufficient data or invalid data length");
            }
        }

        if (this.entry.CompressedSize == this.entry.FileSize)
        {
            this.currentData = filedata;
        }
        else
        {
            this.currentData = DecompressMulti(filedata, (int)this.entry.FileSize);
        }
    }

    private int ReadInternal(byte[] buffer, int offset, int count)
    {
        // OW: avoid reading past the contents of the file
        if (this.position >= this.Length)
        {
            return 0;
        }

        this.BufferData();

        int localposition = (int)(this.position % this.blockSize);
        int bytestocopy = Math.Min(this.currentData.Length - localposition, count);
        if (bytestocopy <= 0)
        {
            return 0;
        }

        Array.Copy(this.currentData, localposition, buffer, offset, bytestocopy);

        this.position += bytestocopy;
        return bytestocopy;
    }

    private int ReadByteSingleUnit()
    {
        if (this.currentData == null)
        {
            this.LoadSingleUnit();
        }

        return this.currentData[this.position++];
    }

    private void BufferData()
    {
        int requiredblock = (int)(this.position / this.blockSize);
        if (requiredblock != this.currentBlockIndex)
        {
            int expectedlength = (int)Math.Min(this.Length - (requiredblock * this.blockSize), this.blockSize);
            this.currentData = this.LoadBlock(requiredblock, expectedlength);
            this.currentBlockIndex = requiredblock;
        }
    }
}

// <copyright file="MpqArchive.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Tools.Mpq;

public class MpqArchive : IDisposable, IEnumerable<MpqEntry>
{
    private static readonly uint[] StormBuffer;

    private MpqHeader? mpqHeader;
    private long headerOffset;
    private MpqHash[] ? hashes;
    private MpqEntry[] ? entries;
    private bool disposedValue;

    static MpqArchive()
    {
        StormBuffer = BuildStormBuffer();
    }

    public MpqArchive(string filename)
    {
        this.BaseStream = File.Open(filename, FileMode.Open, FileAccess.Read);

        try
        {
            this.Init();
        }
        catch
        {
            this.Dispose();
            throw;
        }
    }

    public MpqArchive(Stream sourceStream, bool loadListfile = false)
    {
        this.BaseStream = sourceStream;
        this.Init();

        if (loadListfile)
        {
            this.AddListfileFilenames();
        }
    }

    public int BlockSize { get; private set; }

    public int? Count => this.entries?.Length;

    public MpqHeader? Header => this.mpqHeader;

    internal Stream BaseStream { get; }

    public MpqEntry? this[int index]
    {
        get
        {
            if (this.entries is null)
            {
                return null;
            }

            if (index >= this.entries.Length)
            {
                return null;
            }

            return this.entries[index];
        }
    }

    public MpqEntry? this[string filename]
    {
        get
        {
            if (this.entries is null || !this.TryGetHashEntry(filename, out MpqHash hash))
            {
                return null;
            }

            if (hash.BlockIndex >= this.entries.Length)
            {
                return null;
            }

            return this.entries[hash.BlockIndex];
        }
    }

    public MpqStream? OpenFile(string filename)
    {
        MpqEntry entry;

        if (!this.TryGetHashEntry(filename, out MpqHash hash))
        {
            throw new FileNotFoundException("File not found: " + filename);
        }

        if (this.entries is null)
        {
            return null;
        }

        entry = this.entries[hash.BlockIndex];
        if (string.IsNullOrEmpty(entry.FileName))
        {
            entry.FileName = filename;
        }

        return new MpqStream(this, entry);
    }

    public MpqStream OpenFile(MpqEntry entry)
    {
        return new MpqStream(this, entry);
    }

    public bool FileExists(string filename)
    {
        return this.TryGetHashEntry(filename, out _);
    }

    public bool AddListfileFilenames()
    {
        if (!this.AddFilename("(listfile)"))
        {
            return false;
        }

        using (Stream? s = this.OpenFile("(listfile)"))
        {
            if (s is null)
            {
                return false;
            }

            this.AddFilenames(s);
        }

        return true;
    }

    public void AddFilenames(Stream stream)
    {
        using StreamReader sr = new (stream);

        while (!sr.EndOfStream)
        {
            var line = sr.ReadLine();

            if (line is null)
            {
                break;
            }

            this.AddFilename(line);
        }
    }

    public bool AddFilename(string filename)
    {
        if (this.entries is null || !this.TryGetHashEntry(filename, out MpqHash hash))
        {
            return false;
        }

        this.entries[hash.BlockIndex].FileName = filename;

        return true;
    }

    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.entries.GetEnumerator();
    }

    IEnumerator<MpqEntry> IEnumerable<MpqEntry>.GetEnumerator()
    {
        if (this.entries is null)
        {
            yield break;
        }

        foreach (MpqEntry entry in this.entries)
        {
            yield return entry;
        }
    }

    internal static uint HashString(string input, int offset)
    {
        uint seed1 = 0x7fed7fed;
        uint seed2 = 0xeeeeeeee;

        foreach (char c in input)
        {
            int val = (int)char.ToUpperInvariant(c);
            seed1 = StormBuffer[offset + val] ^ (seed1 + seed2);
            seed2 = (uint)val + seed1 + seed2 + (seed2 << 5) + 3;
        }

        return seed1;
    }

    // Used for Hash Tables and Block Tables
    internal static void DecryptTable(byte[] data, string key)
    {
        DecryptBlock(data, HashString(key, 0x300));
    }

    internal static void DecryptBlock(byte[] data, uint seed1)
    {
        uint seed2 = 0xeeeeeeee;

        // NB: If the block is not an even multiple of 4,
        // the remainder is not encrypted
        for (int i = 0; i < data.Length - 3; i += 4)
        {
            seed2 += StormBuffer[0x400 + (seed1 & 0xff)];

            uint result = BitConverter.ToUInt32(data, i);
            result ^= seed1 + seed2;

            seed1 = ((~seed1 << 21) + 0x11111111) | (seed1 >> 11);
            seed2 = result + seed2 + (seed2 << 5) + 3;

            data[i + 0] = (byte)(result & 0xff);
            data[i + 1] = (byte)((result >> 8) & 0xff);
            data[i + 2] = (byte)((result >> 16) & 0xff);
            data[i + 3] = (byte)((result >> 24) & 0xff);
        }
    }

    internal static void DecryptBlock(uint[] data, uint seed1)
    {
        uint seed2 = 0xeeeeeeee;

        for (int i = 0; i < data.Length; i++)
        {
            seed2 += StormBuffer[0x400 + (seed1 & 0xff)];
            uint result = data[i];
            result ^= seed1 + seed2;

            seed1 = ((~seed1 << 21) + 0x11111111) | (seed1 >> 11);
            seed2 = result + seed2 + (seed2 << 5) + 3;
            data[i] = result;
        }
    }

    // This function calculates the encryption key based on
    // some assumptions we can make about the headers for encrypted files
    internal static uint DetectFileSeed(uint value0, uint value1, uint decrypted)
    {
        uint temp = (value0 ^ decrypted) - 0xeeeeeeee;

        for (int i = 0; i < 0x100; i++)
        {
            uint seed1 = temp - StormBuffer[0x400 + i];
            uint seed2 = 0xeeeeeeee + StormBuffer[0x400 + (seed1 & 0xff)];
            uint result = value0 ^ (seed1 + seed2);

            if (result != decrypted)
            {
                continue;
            }

            uint saveseed1 = seed1;

            // Test this result against the 2nd value
            seed1 = ((~seed1 << 21) + 0x11111111) | (seed1 >> 11);
            seed2 = result + seed2 + (seed2 << 5) + 3;

            seed2 += StormBuffer[0x400 + (seed1 & 0xff)];
            result = value1 ^ (seed1 + seed2);

            if ((result & 0xfffc0000) == 0)
            {
                return saveseed1;
            }
        }

        return 0;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.BaseStream?.Dispose();
            }

            this.disposedValue = true;
        }
    }

    private static uint[] BuildStormBuffer()
    {
        uint seed = 0x100001;

        uint[] result = new uint[0x500];

        for (uint index1 = 0; index1 < 0x100; index1++)
        {
            uint index2 = index1;
            for (int i = 0; i < 5; i++, index2 += 0x100)
            {
                seed = ((seed * 125) + 3) % 0x2aaaab;
                uint temp = (seed & 0xffff) << 16;
                seed = ((seed * 125) + 3) % 0x2aaaab;

                result[index2] = temp | (seed & 0xffff);
            }
        }

        return result;
    }

    private void Init()
    {
        if (!this.LocateMpqHeader())
        {
            throw new MpqParserException("Unable to find MPQ header");
        }

        if (this.mpqHeader is null)
        {
            return;
        }

        if (this.mpqHeader.HashTableOffsetHigh != 0 || this.mpqHeader.ExtendedBlockTableOffset != 0 || this.mpqHeader.BlockTableOffsetHigh != 0)
        {
            throw new MpqParserException("MPQ format version 1 features are not supported");
        }

        BinaryReader br = new (this.BaseStream);

        this.BlockSize = 0x200 << this.mpqHeader.BlockSize;

        // Load hash table
        this.BaseStream.Seek(this.mpqHeader.HashTablePos, SeekOrigin.Begin);
        byte[] hashdata = br.ReadBytes((int)(this.mpqHeader.HashTableSize * MpqHash.Size));
        DecryptTable(hashdata, "(hash table)");

        BinaryReader br2 = new (new MemoryStream(hashdata));
        this.hashes = new MpqHash[this.mpqHeader.HashTableSize];

        for (int i = 0; i < this.mpqHeader.HashTableSize; i++)
        {
            this.hashes[i] = new MpqHash(br2);
        }

        // Load entry table
        this.BaseStream.Seek(this.mpqHeader.BlockTablePos, SeekOrigin.Begin);
        byte[] entrydata = br.ReadBytes((int)(this.mpqHeader.BlockTableSize * MpqEntry.Size));
        DecryptTable(entrydata, "(block table)");

        br2 = new BinaryReader(new MemoryStream(entrydata));
        this.entries = new MpqEntry[this.mpqHeader.BlockTableSize];

        for (int i = 0; i < this.mpqHeader.BlockTableSize; i++)
        {
            this.entries[i] = new MpqEntry(br2, (uint)this.headerOffset);
        }
    }

    private bool LocateMpqHeader()
    {
        BinaryReader br = new (this.BaseStream);

        // In .mpq files the header will be at the start of the file
        // In .exe files, it will be at a multiple of 0x200
        for (long i = 0; i < this.BaseStream.Length - MpqHeader.Size; i += 0x200)
        {
            this.BaseStream.Seek(i, SeekOrigin.Begin);
            this.mpqHeader = MpqHeader.FromReader(br);
            if (this.mpqHeader is not null)
            {
                this.headerOffset = i;
                this.mpqHeader.SetHeaderOffset(this.headerOffset);
                return true;
            }
        }

        return false;
    }

    private bool TryGetHashEntry(string filename, out MpqHash hash)
    {
        hash = default;

        if (this.mpqHeader is null || this.hashes is null)
        {
            return false;
        }

        uint index = HashString(filename, 0);
        index &= this.mpqHeader.HashTableSize - 1;
        uint name1 = HashString(filename, 0x100);
        uint name2 = HashString(filename, 0x200);

        for (uint i = index; i < this.hashes.Length; ++i)
        {
            hash = this.hashes[i];
            if (hash.Name1 == name1 && hash.Name2 == name2)
            {
                return true;
            }
        }

        for (uint i = 0; i < index; i++)
        {
            hash = this.hashes[i];
            if (hash.Name1 == name1 && hash.Name2 == name2)
            {
                return true;
            }
        }

        return false;
    }
}

// <copyright file="MpqEntry.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Tools.Mpq;

public class MpqEntry
{
    public static readonly uint Size = 16;

    /// <summary>
    /// Relative to the header offset.
    /// </summary>
    private readonly uint fileOffset;
    private string? fileName;

    public MpqEntry(BinaryReader br, uint headerOffset)
    {
        this.fileOffset = br.ReadUInt32();
        this.FilePos = headerOffset + this.fileOffset;
        this.CompressedSize = br.ReadUInt32();
        this.FileSize = br.ReadUInt32();
        this.Flags = (MpqFileFlags)br.ReadUInt32();
        this.EncryptionSeed = 0;
    }

    public uint CompressedSize { get; }

    public uint FileSize { get; }

    /// <summary>
    /// Gets absolute position in the file.
    /// </summary>
    public uint FilePos { get; }

    public MpqFileFlags Flags { get; internal set; }

    public uint EncryptionSeed { get; internal set; }

    public bool IsEncrypted => (this.Flags & MpqFileFlags.Encrypted) != 0;

    public bool IsCompressed => (this.Flags & MpqFileFlags.Compressed) != 0;

    public bool Exists => this.Flags != 0;

    public bool IsSingleUnit => (this.Flags & MpqFileFlags.SingleUnit) != 0;

    public string? FileName
    {
        get => this.fileName;
        set
        {
            this.fileName = value;
            this.EncryptionSeed = this.CalculateEncryptionSeed();
        }
    }

    public override string ToString()
    {
        if (this.FileName is null)
        {
            if (!this.Exists)
            {
                return "(Deleted file)";
            }

            return $"Unknown file @ {this.FilePos}";
        }

        return this.FileName;
    }

    private uint CalculateEncryptionSeed()
    {
        if (this.FileName is null)
        {
            return 0;
        }

        uint seed = MpqArchive.HashString(Path.GetFileName(this.FileName), 0x300);
        if ((this.Flags & MpqFileFlags.BlockOffsetAdjustedKey) == MpqFileFlags.BlockOffsetAdjustedKey)
        {
            seed = (seed + this.fileOffset) ^ this.FileSize;
        }

        return seed;
    }
}

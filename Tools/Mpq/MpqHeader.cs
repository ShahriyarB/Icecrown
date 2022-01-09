// <copyright file="MpqHeader.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Tools.Mpq;

public class MpqHeader
{
    public static readonly uint MpqId = 0x1a51504d;
    public static readonly uint Size = 32;

    /// <summary>
    /// Gets signature (Should be 0x1a51504d).
    /// </summary>
    public uint ID { get; private set; }

    /// <summary>
    /// Gets offset of the first file AKA Header size.
    /// </summary>
    public uint DataOffset { get; private set; }

    public uint ArchiveSize { get; private set; }

    /// <summary>
    /// Gets mpq version.
    /// Most are 0, Burning Crusade = 1.
    /// </summary>
    public ushort MpqVersion { get; private set; }

    /// <summary>
    /// Gets size of file block is (0x200 << BlockSize).
    /// </summary>
    public ushort BlockSize { get; private set; }

    public uint HashTablePos { get; private set; }

    public uint BlockTablePos { get; private set; }

    public uint HashTableSize { get; private set; }

    public uint BlockTableSize { get; private set; }

    /// <summary>
    /// Gets Version 1 fields.
    /// The extended block table is an array of Int16 - higher bits of the offests in the block table.
    /// </summary>
    public long ExtendedBlockTableOffset { get; private set; }

    public short HashTableOffsetHigh { get; private set; }

    public short BlockTableOffsetHigh { get; private set; }

    public static MpqHeader? FromReader(BinaryReader br)
    {
        uint id = br.ReadUInt32();

        if (id != MpqId)
        {
            return null;
        }

        MpqHeader header = new ()
        {
            ID = id,
            DataOffset = br.ReadUInt32(),
            ArchiveSize = br.ReadUInt32(),
            MpqVersion = br.ReadUInt16(),
            BlockSize = br.ReadUInt16(),
            HashTablePos = br.ReadUInt32(),
            BlockTablePos = br.ReadUInt32(),
            HashTableSize = br.ReadUInt32(),
            BlockTableSize = br.ReadUInt32(),
        };

        if (header.MpqVersion == 1)
        {
            header.ExtendedBlockTableOffset = br.ReadInt64();
            header.HashTableOffsetHigh = br.ReadInt16();
            header.BlockTableOffsetHigh = br.ReadInt16();
        }

        return header;
    }

    public void SetHeaderOffset(long headerOffset)
    {
        this.HashTablePos += (uint)headerOffset;
        this.BlockTablePos += (uint)headerOffset;

        // A protected archive
        // Seen in some custom wc3 maps.
        if (this.DataOffset == 0x6d9e4b86)
        {
            this.DataOffset = (uint)(Size + headerOffset);
        }
    }
}

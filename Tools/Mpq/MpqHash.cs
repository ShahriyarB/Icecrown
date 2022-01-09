// <copyright file="MpqHash.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Tools.Mpq;

public struct MpqHash
{
    public static readonly uint Size = 16;

    public MpqHash(BinaryReader br)
        : this()
    {
        this.Name1 = br.ReadUInt32();
        this.Name2 = br.ReadUInt32();
        this.Locale = br.ReadUInt32(); // Normally 0 or UInt32.MaxValue (0xffffffff)
        this.BlockIndex = br.ReadUInt32();
    }

    public uint Name1 { get; }

    public uint Name2 { get; }

    public uint Locale { get; }

    public uint BlockIndex { get; }
}

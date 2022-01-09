// <copyright file="MpqFileFlags.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Tools.Mpq;

[Flags]
public enum MpqFileFlags : uint
{
    None = 0,
    CompressedPK = 0x100,
    CompressedMulti = 0x200,
    EnumMember = 1024,
    EnumMember2 = 2048,
    EnumMember3 = 4096,
    EnumMember4 = 8192,
    EnumMember5 = 16384,
    EnumMember6 = 32768,
    Compressed = CompressedPK | CompressedMulti | EnumMember | EnumMember2 | EnumMember3 | EnumMember4 | EnumMember5 | EnumMember6,
    Encrypted = 0x10000,
    BlockOffsetAdjustedKey = 0x020000,
    SingleUnit = 0x1000000,
    FileHasMetadata = 0x04000000,
    Exists = 0x80000000,
}

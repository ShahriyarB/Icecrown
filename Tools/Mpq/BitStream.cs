// <copyright file="BitStream.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Tools.Mpq;

internal class BitStream
{
    private readonly Stream baseStream;
    private int current;
    private int bitCount;

    public BitStream(Stream sourceStream)
    {
        this.baseStream = sourceStream;
    }

    public int ReadBits(int bitCount)
    {
        if (bitCount > 16)
        {
            throw new ArgumentOutOfRangeException(nameof(bitCount), "Maximum BitCount is 16");
        }

        if (!this.EnsureBits(bitCount))
        {
            return -1;
        }

        int result = this.current & (0xffff >> (16 - bitCount));
        this.WasteBits(bitCount);
        return result;
    }

    public int PeekByte()
    {
        return !this.EnsureBits(8) ? -1 : this.current & 0xff;
    }

    public bool EnsureBits(int bitCount)
    {
        if (bitCount <= this.bitCount)
        {
            return true;
        }

        if (this.baseStream.Position >= this.baseStream.Length)
        {
            return false;
        }

        int nextvalue = this.baseStream.ReadByte();
        this.current |= nextvalue << this.bitCount;
        this.bitCount += 8;
        return true;
    }

    private bool WasteBits(int bitCount)
    {
        this.current >>= bitCount;
        this.bitCount -= bitCount;
        return true;
    }
}

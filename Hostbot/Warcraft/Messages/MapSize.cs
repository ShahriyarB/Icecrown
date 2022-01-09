// <copyright file="MapSize.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS_MAPSIZE
/// Transport Layer:          Transmission Control Protocol (TCP)
/// Application Layer:        Warcraft III In-Game Messages (W3GS)
/// Message Id:               0x42 (66)
/// Direction:                Client to Server
/// Used By:                  Warcraft III Reign of Chaos, Warcraft III The Frozen Throne.
/// </summary>
internal class MapSize : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapSize"/> class.
    /// </summary>
    /// <param name="memory">Memory region containing message data.</param>
    public MapSize(Memory<byte> memory)
        : base(memory)
    {
        this.Unknown = this.Reader.ReadUInt32();
        this.Flag = this.Reader.ReadByte();
        this.SizeInBytes = this.Reader.ReadUInt32();
    }

    /// <summary>
    /// Gets unknown value.
    /// </summary>
    internal uint Unknown { get; }

    /// <summary>
    /// Gets size flag.
    /// </summary>
    internal byte Flag { get; }

    /// <summary>
    /// Gets map size.
    /// </summary>
    internal uint SizeInBytes { get; }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        using var stream = new MemoryStream(9);
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(this.Unknown);
            writer.Write(this.Flag);
            writer.Write(this.SizeInBytes);
        }

        return stream.ToArray();
    }
}

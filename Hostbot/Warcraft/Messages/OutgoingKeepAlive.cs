// <copyright file="OutgoingKeepAlive.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS_OUTGOING_KEEPALIVE
/// Transport Layer:          Transmission Control Protocol (TCP)
/// Application Layer:        Warcraft III In-Game Messages (W3GS)
/// Message Id:               0x27 (39)
/// Direction:                Client to Server.
/// </summary>
internal class OutgoingKeepAlive : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OutgoingKeepAlive"/> class.
    /// </summary>
    /// <param name="memory">Memory region containing message data.</param>
    public OutgoingKeepAlive(Memory<byte> memory)
        : base(memory)
    {
        this.Unknown = this.Reader.ReadByte();
        this.Checksum = this.Reader.ReadUInt32();
    }

    /// <summary>
    /// Gets unknown byte.
    /// </summary>
    internal byte Unknown { get; }

    /// <summary>
    /// Gets checksum.
    /// </summary>
    internal uint Checksum { get; }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        using var stream = new MemoryStream(5);
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(this.Unknown);
            writer.Write(this.Checksum);
        }

        return stream.ToArray();
    }
}

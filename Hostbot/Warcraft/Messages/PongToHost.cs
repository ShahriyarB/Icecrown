// <copyright file="PongToHost.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS_PONG_TO_HOST
/// Transport Layer:          Transmission Control Protocol (TCP)
/// Application Layer:        Warcraft III In-Game Messages (W3GS)
/// Message Id:               0x46 (70)
/// Direction:                Client to Server
/// Used By:                  Warcraft III Reign of Chaos, Warcraft III The Frozen Throne.
/// </summary>
internal class PongToHost : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PongToHost"/> class.
    /// </summary>
    /// <param name="memory">Memory region containing message data.</param>
    public PongToHost(Memory<byte> memory)
        : base(memory)
    {
        this.Ticks = this.Reader.ReadUInt32();
    }

    /// <summary>
    /// Gets ping ticks.
    /// </summary>
    internal uint Ticks { get; }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        using var stream = new MemoryStream(4);
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(this.Ticks);
        }

        return stream.ToArray();
    }
}

// <copyright file="PingFromHost.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS_PING_FROM_HOST
/// Transport Layer:          Transmission Control Protocol (TCP)
/// Application Layer:        Warcraft III In-Game Messages (W3GS)
/// Message Id:               0x01 (1)
/// Direction:                Server to Client
/// Used By:                  Warcraft III Reign of Chaos, Warcraft III The Frozen Throne.
/// </summary>
internal class PingFromHost : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PingFromHost"/> class.
    /// </summary>
    /// <param name="ticks">Ping time in ms.</param>
    public PingFromHost(uint ticks)
    {
        this.Type = GameProtocol.W3GSHeaderConstant;
        this.Id = GameProtocol.W3GSPingFromHost;
        this.Ticks = ticks;
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

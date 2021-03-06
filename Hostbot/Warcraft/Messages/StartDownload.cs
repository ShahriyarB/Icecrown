// <copyright file="StartDownload.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS_STARTDOWNLOAD
/// Transport Layer:          Transmission Control Protocol (TCP)
/// Application Layer:        Warcraft III In-Game Messages (W3GS)
/// Message Id:               0x3F (63)
/// Direction:                Server to Client
/// Used By:                  Warcraft III Reign of Chaos, Warcraft III The Frozen Throne.
/// </summary>
internal class StartDownload : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartDownload"/> class.
    /// </summary>
    /// <param name="fromPlayerId">From player id.</param>
    public StartDownload(byte fromPlayerId)
    {
        this.Type = GameProtocol.W3GSHeaderConstant;
        this.Id = GameProtocol.W3GSStartDownload;
        this.FromPlayerId = fromPlayerId;
    }

    /// <summary>
    /// Gets from player id.
    /// </summary>
    internal byte FromPlayerId { get; }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        using var stream = new MemoryStream(5);
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(1);
            writer.Write(this.FromPlayerId);
        }

        return stream.ToArray();
    }
}

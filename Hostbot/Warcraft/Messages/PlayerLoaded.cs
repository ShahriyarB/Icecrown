// <copyright file="PlayerLoaded.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS_PLAYERLOADED
/// Transport Layer:          Transmission Control Protocol (TCP)
/// Application Layer:        Warcraft III In-Game Messages (W3GS)
/// Message Id:               0x08 (8)
/// Direction:                Server to Client
/// Used By:                  Warcraft III Reign of Chaos, Warcraft III The Frozen Throne.
/// </summary>
internal class PlayerLoaded : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerLoaded"/> class.
    /// </summary>
    /// <param name="playerId">Player id.</param>
    public PlayerLoaded(byte playerId)
    {
        this.Type = GameProtocol.W3GSHeaderConstant;
        this.Id = GameProtocol.W3GSPlayerLoaded;
        this.PlayerId = playerId;
    }

    /// <summary>
    /// Gets player id.
    /// </summary>
    internal byte PlayerId { get; }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        return new byte[] { this.PlayerId };
    }
}

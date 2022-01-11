// <copyright file="CountdownStart.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS_COUNTDOWN_START
/// Transport Layer:          Transmission Control Protocol (TCP)
/// Application Layer:        Warcraft III In-Game Messages (W3GS)
/// Message Id:               0x0A (10)
/// Direction:                Server to Client
/// Used By:                  Warcraft III Reign of Chaos, Warcraft III The Frozen Throne.
/// </summary>
internal class CountdownStart : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CountdownStart"/> class.
    /// </summary>
    public CountdownStart()
    {
        this.Type = GameProtocol.W3GSHeaderConstant;
        this.Id = GameProtocol.W3GSCountdownStart;
    }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        return Array.Empty<byte>();
    }
}

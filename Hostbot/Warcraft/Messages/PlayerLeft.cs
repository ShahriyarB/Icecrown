// <copyright file="PlayerLeft.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS_PLAYERLEFT
/// Transport Layer:          Transmission Control Protocol (TCP)
/// Application Layer:        Warcraft III In-Game Messages (W3GS)
/// Message Id:               0x07 (7)
/// Direction:                Server to Client
/// Used By:                  Warcraft III Reign of Chaos, Warcraft III The Frozen Throne.
/// </summary>
internal class PlayerLeft : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerLeft"/> class.
    /// </summary>
    /// <param name="playerId">Player id.</param>
    /// <param name="reason">Player leave reason.</param>
    public PlayerLeft(byte playerId, PlayerLeaveReason reason)
    {
        this.Type = GameProtocol.W3GSHeaderConstant;
        this.Id = GameProtocol.W3GSPlayerLeft;
        this.PlayerId = playerId;
        this.Reason = reason;
    }

    /// <summary>
    /// Gets player id.
    /// </summary>
    internal byte PlayerId { get; }

    /// <summary>
    /// Gets player leave reason.
    /// </summary>
    internal PlayerLeaveReason Reason { get; }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        using var stream = new MemoryStream(5);
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(this.PlayerId);
            writer.Write((uint)this.Reason);
        }

        return stream.ToArray();
    }
}

// <copyright file="PlayerInfo.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS_PLAYERINFO
/// Transport Layer:          Transmission Control Protocol (TCP)
/// Application Layer:        Warcraft III In-Game Messages (W3GS)
/// Message Id:               0x06 (6)
/// Direction:                Server to Client
/// Used By:                  Warcraft III Reign of Chaos, Warcraft III The Frozen Throne.
/// </summary>
internal class PlayerInfo : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerInfo"/> class.
    /// </summary>
    /// <param name="playerId">Player id.</param>
    /// <param name="name">Player name.</param>
    /// <param name="externalIp">Player external ip.</param>
    /// <param name="internalIp">Player internal ip.</param>
    public PlayerInfo(byte playerId, string name, uint externalIp, uint internalIp)
    {
        this.Type = GameProtocol.W3GSHeaderConstant;
        this.Id = GameProtocol.W3GSPlayerInfo;
        this.PlayerId = playerId;
        this.Name = name;
        this.ExternalIp = externalIp;
        this.InternalIp = internalIp;
    }

    /// <summary>
    /// Gets player id.
    /// </summary>
    internal byte PlayerId { get; }

    /// <summary>
    /// Gets player name.
    /// </summary>
    internal string Name { get; }

    /// <summary>
    /// Gets player external ip.
    /// </summary>
    internal uint ExternalIp { get; }

    /// <summary>
    /// Gets player internal ip.
    /// </summary>
    internal uint InternalIp { get; }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        using var stream = new MemoryStream(39);
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(2);
            writer.Write(this.PlayerId);
            writer.WriteString(this.Name);
            writer.Write((ushort)1);
            writer.Write(2);
            writer.Write(this.ExternalIp);
            writer.Write(0L);
            writer.Write(2);
            writer.Write(this.InternalIp);
            writer.Write(0L);
        }

        return stream.ToArray();
    }
}

// <copyright file="SlotInfoJoin.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS_SLOTINFOJOIN
/// Transport Layer:          Transmission Control Protocol (TCP)
/// Application Layer:        Warcraft III In-Game Messages (W3GS)
/// Message Id:               0x04 (4)
/// Direction:                Server to Client
/// Used By:                  Warcraft III Reign of Chaos, Warcraft III The Frozen Throne.
/// </summary>
internal class SlotInfoJoin : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SlotInfoJoin"/> class.
    /// </summary>
    /// <param name="playerId">Player id.</param>
    /// <param name="port">Player port.</param>
    /// <param name="externalIp">Player external ip.</param>
    /// <param name="slots">Game slots.</param>
    /// <param name="randomSeed">Random seed.</param>
    /// <param name="layoutStyle">Layout style.</param>
    /// <param name="playerSlots">Player slots.</param>
    public SlotInfoJoin(byte playerId, ushort port, uint externalIp, List<IGameSlot> slots, uint randomSeed, byte layoutStyle, byte playerSlots)
    {
        this.Type = GameProtocol.W3GSHeaderConstant;
        this.Id = GameProtocol.W3GSSlotInfoJoin;
        this.PlayerId = playerId;
        this.Port = port;
        this.ExternalIp = externalIp;
        this.Slots = slots;
        this.RandomSeed = randomSeed;
        this.LayoutStyle = layoutStyle;
        this.PlayerSlots = playerSlots;
    }

    /// <summary>
    /// Gets player id.
    /// </summary>
    internal byte PlayerId { get; }

    /// <summary>
    /// Gets player port.
    /// </summary>
    internal ushort Port { get; }

    /// <summary>
    /// Gets player external ip.
    /// </summary>
    internal uint ExternalIp { get; }

    /// <summary>
    /// Gets game slots.
    /// </summary>
    internal List<IGameSlot> Slots { get; }

    /// <summary>
    /// Gets random seed.
    /// </summary>
    internal uint RandomSeed { get; }

    /// <summary>
    /// Gets layout style.
    /// </summary>
    internal byte LayoutStyle { get; }

    /// <summary>
    /// Gets player slots.
    /// </summary>
    internal byte PlayerSlots { get; }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        using var stream = new MemoryStream(19 + (this.Slots.Count * 16));
        using (var writer = new BinaryWriter(stream))
        {
            var slotInfo = Utility.EncodeSlotInfo(this.Slots, this.RandomSeed, this.LayoutStyle, this.PlayerSlots);

            writer.Write((ushort)slotInfo.Length);
            writer.Write(slotInfo);
            writer.Write(this.PlayerId);
            writer.Write((ushort)2);
            writer.Write(this.Port);
            writer.Write(this.ExternalIp);
            writer.Write(0L);
        }

        return stream.ToArray();
    }
}

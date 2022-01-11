// <copyright file="SlotInfo.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS_SLOTINFO
/// Transport Layer:          Transmission Control Protocol (TCP)
/// Application Layer:        Warcraft III In-Game Messages (W3GS)
/// Message Id:               0x09 (9)
/// Direction:                Server to Client
/// Used By:                  Warcraft III Reign of Chaos, Warcraft III The Frozen Throne.
/// </summary>
internal class SlotInfo : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SlotInfo"/> class.
    /// </summary>
    /// <param name="slots">Game slots.</param>
    /// <param name="randomSeed">Random seed.</param>
    /// <param name="layoutStyle">Layout style.</param>
    /// <param name="playerSlots">Player slots.</param>
    public SlotInfo(List<IGameSlot> slots, uint randomSeed, byte layoutStyle, byte playerSlots)
    {
        this.Type = GameProtocol.W3GSHeaderConstant;
        this.Id = GameProtocol.W3GSSlotInfo;
        this.Slots = slots;
        this.RandomSeed = randomSeed;
        this.LayoutStyle = layoutStyle;
        this.PlayerSlots = playerSlots;
    }

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
        using var stream = new MemoryStream(2 + (this.Slots.Count * 16));
        using (var writer = new BinaryWriter(stream))
        {
            var slotInfo = Utility.EncodeSlotInfo(this.Slots, this.RandomSeed, this.LayoutStyle, this.PlayerSlots);

            writer.Write((ushort)slotInfo.Length);
            writer.Write(slotInfo);
        }

        return stream.ToArray();
    }
}

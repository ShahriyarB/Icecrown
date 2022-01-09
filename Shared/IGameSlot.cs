// <copyright file="IGameSlot.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Shared;

using Icecrown.Shared.Warcraft;

/// <summary>
/// Game slot interface.
/// </summary>
public interface IGameSlot
{
    /// <summary>
    /// Gets slot index.
    /// </summary>
    byte SlotIndex { get; }

    /// <summary>
    /// Gets player id associated with this slot.
    /// </summary>
    byte PlayerId { get; }

    /// <summary>
    /// Gets or sets player instance associated with this slot.
    /// </summary>
    IPlayer? Player { get; set; }

    /// <summary>
    /// Gets or sets download status (0% to 100%).
    /// </summary>
    byte DownloadStatus { get; set; }

    /// <summary>
    /// Gets or sets slot status.
    /// </summary>
    SlotStatus SlotStatus { get; set; }

    /// <summary>
    /// Gets or sets computer value (0 = no, 1 = yes).
    /// </summary>
    byte Computer { get; set; }

    /// <summary>
    /// Gets or sets team.
    /// </summary>
    byte Team { get; set; }

    /// <summary>
    /// Gets or sets color.
    /// </summary>
    byte Color { get; set; }

    /// <summary>
    /// Gets or sets race.
    /// </summary>
    SlotRace Race { get; set; }

    /// <summary>
    /// Gets or sets computer type.
    /// </summary>
    SlotComputerType ComputerType { get; set; }

    /// <summary>
    /// Gets or sets handicap.
    /// </summary>
    byte Handicap { get; set; }

    /// <summary>
    /// Tries to open this slot.
    /// </summary>
    /// <returns>Returns true if the slot opened successfully.</returns>
    bool Open();

    /// <summary>
    /// Tries to close this slot.
    /// </summary>
    /// <returns>Returns true if the slot closed successfully.</returns>
    bool Close();

    /// <summary>
    /// Tries to swap this slot with another slot.
    /// </summary>
    /// <param name="other">Other slot.</param>
    /// <returns>Returns true if swap completed successfully.</returns>
    bool Swap(IGameSlot other);

    /// <summary>
    /// Converts this slot to a warcraft readable byte array.
    /// </summary>
    /// <returns>Returns byte array of this game slot.</returns>
    byte[] ToByteArray();
}

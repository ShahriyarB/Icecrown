// <copyright file="IPlayer.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Shared;

using Icecrown.Shared.Warcraft;

/// <summary>
/// Player interface.
/// </summary>
public interface IPlayer
{
    /// <summary>
    /// Gets or sets player id.
    /// </summary>
    byte Id { get; set; }

    /// <summary>
    /// Gets or sets player name.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets or sets player slot.
    /// </summary>
    IGameSlot? Slot { get; set; }

    /// <summary>
    /// Gets or sets ticks of when this player finished loading game.
    /// </summary>
    long FinishedLoadingTicks { get; set; }

    /// <summary>
    /// Gets a value indicating whether this player finished loading or not.
    /// </summary>
    bool IsLoadingFinished { get; }

    /// <summary>
    /// Gets a value indicating whether this player is dummy or real.
    /// </summary>
    bool IsDummy { get; }

    /// <summary>
    /// Gets a value indicating whether this player is connected or not (join request accepted or not).
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets a value indicating whether this player should be deleted on the next server tick or not.
    /// </summary>
    bool ShouldDelete { get; }

    /// <summary>
    /// Gets player ping.
    /// </summary>
    /// <returns>Returns player average ping.</returns>
    uint GetPing();

    /// <summary>
    /// Send a private chat message to this player from hostbot virtual host.
    /// </summary>
    /// <param name="message">Chat message.</param>
    void SendChat(string message);

    /// <summary>
    /// Tells hostbot to delete this user on next tick.
    /// </summary>
    /// <param name="code">Plauer leave code.</param>
    /// <param name="reason">Player delete reason.</param>
    void Delete(PlayerLeaveReason code, string reason);
}

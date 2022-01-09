// <copyright file="IHostbot.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Shared;

using Icecrown.Shared.Warcraft;

/// <summary>
/// Hostbot interface.
/// </summary>
public interface IHostbot
{
    /// <summary>
    /// Gets current game state.
    /// </summary>
    GameState GameState { get; }

    /// <summary>
    /// Gets or sets game latency (tick rate).
    /// </summary>
    ushort Latency { get; set; }

    /// <summary>
    /// Gets players list.
    /// </summary>
    List<IPlayer> Players { get; }

    /// <summary>
    /// Gets game slots.
    /// </summary>
    List<IGameSlot> Slots { get; }

    /// <summary>
    /// Gets or sets map HCL.
    /// </summary>
    string MapHCL { get; set; }

    /// <summary>
    /// Gets a value indicating whether lag screen is active or not (one or more players are lagging).
    /// </summary>
    bool IsLagging { get; }

    /// <summary>
    /// Attempts to start the game by starting the countdown.
    /// Game will start 5 seconds after this method is called unless <see cref="StopCountdown"/> is called or a player leaves or joins.
    /// </summary>
    /// <returns>Returns true if countdown is started successfully.</returns>
    bool StartCountdown();

    /// <summary>
    /// Stops the countdown.
    /// </summary>
    void StopCountdown();

    /// <summary>
    /// Inserts a dummy to the lobby.
    /// </summary>
    /// <param name="fillRemaining">Whether to fill remaining open slots with dummies or just add a single dummy.</param>
    void InsertDummy(bool fillRemaining);

    /// <summary>
    /// Removes dummy from the lobby.
    /// </summary>
    /// <param name="removeAll">Whether to remove all dummies or just one.</param>
    void RemoveDummy(bool removeAll);

    /// <summary>
    /// Sends a text message to all connected players (both lobby and in game chat supported).
    /// In game chats will be marked as [All] ...
    /// Virtual host pid is used as sender for lobby chats and since we remove virtual host
    /// right before the game starts, we can not use it as sender therefore we use first available player id as sender.
    /// </summary>
    /// <param name="message">Message string.</param>
    void SendAllChat(string message);
}

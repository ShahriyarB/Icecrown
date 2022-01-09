// <copyright file="GameLoadedOthers.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS game loaded others message.
/// </summary>
internal class GameLoadedOthers : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameLoadedOthers"/> class.
    /// </summary>
    /// <param name="playerId">Player id.</param>
    public GameLoadedOthers(byte playerId)
    {
        this.Id = GameProtocol.W3GSGameLoadedOthers;
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

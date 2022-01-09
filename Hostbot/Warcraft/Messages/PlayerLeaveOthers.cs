// <copyright file="PlayerLeaveOthers.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS Player leave message.
/// </summary>
internal class PlayerLeaveOthers : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerLeaveOthers"/> class.
    /// </summary>
    /// <param name="playerId">Player id.</param>
    /// <param name="reason">Player leave reason.</param>
    public PlayerLeaveOthers(byte playerId, PlayerLeaveReason reason)
    {
        this.Id = GameProtocol.W3GSPlayerLeaveOthers;
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

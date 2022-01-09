// <copyright file="OutgoingAction.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS outgoing action message.
/// </summary>
internal class OutgoingAction : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OutgoingAction"/> class.
    /// </summary>
    /// <param name="memory">Memory region containing message data.</param>
    /// <param name="playerId">Player id of action's sender.</param>
    public OutgoingAction(Memory<byte> memory, byte playerId)
        : base(memory)
    {
        this.Crc = this.Reader.ReadUInt32();
        this.Action = this.Reader.ReadBytes(this.Size - 8);
        this.PlayerId = playerId;
    }

    /// <summary>
    /// Gets action crc.
    /// </summary>
    internal uint Crc { get; }

    /// <summary>
    /// Gets action data.
    /// </summary>
    internal byte[] Action { get; }

    /// <summary>
    /// Gets player id.
    /// </summary>
    internal byte PlayerId { get; }

    /// <summary>
    /// Gets unknown length.
    /// TODO: fix summary.
    /// </summary>
    /// <returns>Returns unknown length.</returns>
    internal int GetLength()
    {
        return this.Action.Length + 3;
    }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        using var stream = new MemoryStream(4 + this.Action.Length);
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(this.Crc);
            writer.Write(this.Action);
        }

        return stream.ToArray();
    }
}

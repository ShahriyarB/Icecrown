// <copyright file="RejectJoin.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS Join request message.
/// </summary>
internal class RejectJoin : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RejectJoin"/> class.
    /// </summary>
    /// <param name="reason">Rejection reason.</param>
    public RejectJoin(RejectJoinReason reason)
    {
        this.Id = GameProtocol.W3GSRejectJoin;
        this.Reason = reason;
    }

    /// <summary>
    /// Gets rejection reason.
    /// </summary>
    internal RejectJoinReason Reason { get; }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        using var stream = new MemoryStream(4);
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write((uint)this.Reason);
        }

        return stream.ToArray();
    }
}

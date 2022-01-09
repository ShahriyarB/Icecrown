// <copyright file="LeaveRequest.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS_LEAVEREQ
/// Transport Layer:          Transmission Control Protocol (TCP)
/// Application Layer:        Warcraft III In-Game Messages (W3GS)
/// Message Id:               0x21 (33)
/// Direction:                Client to Server
/// Used By:                  Warcraft III Reign of Chaos, Warcraft III The Frozen Throne.
/// </summary>
internal class LeaveRequest : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LeaveRequest"/> class.
    /// </summary>
    /// <param name="memory">Memory region containing message data.</param>
    public LeaveRequest(Memory<byte> memory)
        : base(memory)
    {
        this.Reason = (PlayerLeaveReason)this.Reader.ReadUInt32();
    }

    /// <summary>
    /// Gets leave reason.
    /// </summary>
    internal PlayerLeaveReason Reason { get; }

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

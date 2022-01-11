// <copyright file="IncomingAction.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS_INCOMING_ACTION
/// Transport Layer:          Transmission Control Protocol (TCP)
/// Application Layer:        Warcraft III In-Game Messages (W3GS)
/// Message Id:               0x0C (12)
/// Direction:                Server to Client
/// Used By:                  Warcraft III Reign of Chaos, Warcraft III The Frozen Throne.
/// </summary>
internal class IncomingAction : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IncomingAction"/> class.
    /// </summary>
    /// <param name="actions">Actions to send.</param>
    /// <param name="sendInterval">Actions send interval.</param>
    public IncomingAction(OutgoingAction[] actions, ushort sendInterval)
    {
        this.Type = GameProtocol.W3GSHeaderConstant;
        this.Id = GameProtocol.W3GSIncomingAction;
        this.Actions = actions;
        this.SendInterval = sendInterval;
    }

    /// <summary>
    /// Gets actions array.
    /// </summary>
    internal OutgoingAction[] Actions { get; }

    /// <summary>
    /// Gets send interval.
    /// </summary>
    internal ushort SendInterval { get; }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        using var stream = new MemoryStream(4 + this.Actions.Sum(a => a.Action.Length + 3));
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(this.SendInterval);

            if (this.Actions.Length > 0)
            {
                using var subStream = new MemoryStream();
                using (var subWriter = new BinaryWriter(subStream, Encoding.Default, true))
                {
                    foreach (var action in this.Actions)
                    {
                        subWriter.Write(action.PlayerId);
                        subWriter.Write((ushort)action.Action.Length);
                        subWriter.Write(action.Action);
                    }
                }

                subStream.Position = 0;
                writer.Write((ushort)new CRC32().GetCrc32(subStream));
                writer.Write(subStream.ToArray());
            }
        }

        return stream.ToArray();
    }
}

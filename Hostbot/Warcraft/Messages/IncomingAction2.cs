// <copyright file="IncomingAction2.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

using Ionic.Crc;

/// <summary>
/// W3GS incoming action 2 message.
/// </summary>
internal class IncomingAction2 : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IncomingAction2"/> class.
    /// </summary>
    /// <param name="actions">Actions to send.</param>
    public IncomingAction2(OutgoingAction[] actions)
    {
        this.Id = GameProtocol.W3GSIncomingAction2;
        this.Actions = actions;
    }

    /// <summary>
    /// Gets actions array.
    /// </summary>
    internal OutgoingAction[] Actions { get; }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        using var stream = new MemoryStream(4 + this.Actions.Sum(a => a.Action.Length + 3));
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write((ushort)0);

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

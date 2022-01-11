// <copyright file="ChatFromHost.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS_CHAT_FROM_HOST
/// Transport Layer:          Transmission Control Protocol (TCP)
/// Application Layer:        Warcraft III In-Game Messages (W3GS)
/// Message Id:               0x0F (15)
/// Direction:                Server to Client
/// Used By:                  Warcraft III Reign of Chaos, Warcraft III The Frozen Throne.
/// </summary>
internal class ChatFromHost : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChatFromHost"/> class.
    /// </summary>
    /// <param name="fromPlayerId">From player id.</param>
    /// <param name="toPlayerIds">To player ids.</param>
    /// <param name="command">Chat command.</param>
    /// <param name="arg">Chat arg (optional).</param>
    /// <param name="extraFlags">Chat extra flags.</param>
    /// <param name="message">Chat message.</param>
    public ChatFromHost(byte fromPlayerId, byte[] toPlayerIds, ChatToHostCommand command, byte? arg, byte[]? extraFlags, string message)
    {
        this.Type = GameProtocol.W3GSHeaderConstant;
        this.Id = GameProtocol.W3GSChatFromHost;
        this.FromPlayerId = fromPlayerId;
        this.ToPlayerIds = toPlayerIds;
        this.Command = command;
        this.Arg = arg;
        this.ExtraFlags = extraFlags;
        this.Message = message;
    }

    /// <summary>
    /// Gets From player id.
    /// </summary>
    internal byte FromPlayerId { get; }

    /// <summary>
    /// Gets to player ids.
    /// </summary>
    internal byte[] ToPlayerIds { get; }

    /// <summary>
    /// Gets message command.
    /// </summary>
    internal ChatToHostCommand Command { get; }

    /// <summary>
    /// Gets message argument if it's not a text command.
    /// </summary>
    internal byte? Arg { get; }

    /// <summary>
    /// Gets chat flag extra.
    /// </summary>
    internal byte[]? ExtraFlags { get; }

    /// <summary>
    /// Gets chat message.
    /// </summary>
    internal string Message { get; }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        using var stream = new MemoryStream(5);
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write((byte)this.ToPlayerIds.Length);
            writer.Write(this.ToPlayerIds);
            writer.Write(this.FromPlayerId);
            writer.Write((byte)this.Command);

            switch (this.Command)
            {
                case ChatToHostCommand.ChangeTeam:
                case ChatToHostCommand.ChangeColor:
                case ChatToHostCommand.ChangeRace:
                case ChatToHostCommand.ChangeHandicap:
                    if (this.Arg is not null)
                    { 
                        writer.Write(this.Arg.Value);
                    }

                    break;
                case ChatToHostCommand.MessageExtra:
                    if (this.ExtraFlags is not null)
                    { 
                        writer.Write(this.ExtraFlags);
                    }

                    break;
            }

            writer.WriteString(this.Message);
        }

        return stream.ToArray();
    }
}

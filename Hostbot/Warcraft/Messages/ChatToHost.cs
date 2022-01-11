// <copyright file="ChatToHost.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS_CHAT_TO_HOST
/// Transport Layer:          Transmission Control Protocol (TCP)
/// Application Layer:        Warcraft III In-Game Messages (W3GS)
/// Message Id:               0x28 (40)
/// Direction:                Client to Server
/// Used By:                  Warcraft III Reign of Chaos, Warcraft III The Frozen Throne.
/// </summary>
internal class ChatToHost : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChatToHost"/> class.
    /// </summary>
    /// <param name="memory">Memory region containing message data.</param>
    public ChatToHost(Memory<byte> memory)
        : base(memory)
    {
        byte len = this.Reader.ReadByte();

        this.ToPlayerIds = new byte[len];

        for (byte i = 0; i < len; i++)
        {
            this.ToPlayerIds[i] = this.Reader.ReadByte();
        }

        this.FromPlayerId = this.Reader.ReadByte();
        this.Command = (ChatToHostCommand)this.Reader.ReadByte();

        switch (this.Command)
        {
            case ChatToHostCommand.ChangeTeam:
            case ChatToHostCommand.ChangeColor:
            case ChatToHostCommand.ChangeRace:
            case ChatToHostCommand.ChangeHandicap:
                this.Arg = this.Reader.ReadByte();
                break;
            case ChatToHostCommand.MessageExtra:
                this.ExtraFlags = this.Reader.ReadBytes(4);
                break;
        }

        this.Message = this.Reader.ReadNullTeminatedString();
    }

    /// <summary>
    /// Gets target player ids.
    /// </summary>
    internal byte[] ToPlayerIds { get; }

    /// <summary>
    /// Gets sender player id.
    /// </summary>
    internal byte FromPlayerId { get; }

    /// <summary>
    /// Gets message command.
    /// </summary>
    internal ChatToHostCommand Command { get; }

    /// <summary>
    /// Gets message argument if it's not a text command.
    /// </summary>
    internal byte? Arg { get; }

    /// <summary>
    /// Gets extra flags if it's a message extra command.
    /// </summary>
    internal byte[]? ExtraFlags { get; }

    /// <summary>
    /// Gets message this if it's a text command.
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

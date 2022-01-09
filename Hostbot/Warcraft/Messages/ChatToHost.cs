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
        var total = this.Reader.ReadByte();

        this.ToPlayerIds = new byte[total];

        for (byte i = 0; i < total; i++)
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
            case ChatToHostCommand.Message:
                this.Text = this.Reader.ReadNullTeminatedString();
                break;
            case ChatToHostCommand.MessageExtra:
                this.ExtraFlags = this.Reader.ReadBytes(4);
                this.Text = this.Reader.ReadNullTeminatedString();
                break;
        }
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
    internal string? Text { get; }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        using var stream = new MemoryStream(5);
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write((byte)this.ToPlayerIds.Length);

            foreach (var playerId in this.ToPlayerIds)
            {
                writer.Write(playerId);
            }

            writer.Write(this.FromPlayerId);
            writer.Write((byte)this.Command);

            switch (this.Command)
            {
                case ChatToHostCommand.ChangeTeam:
                case ChatToHostCommand.ChangeColor:
                case ChatToHostCommand.ChangeRace:
                case ChatToHostCommand.ChangeHandicap:
                    writer.Write(this.Arg ?? default);
                    break;
                case ChatToHostCommand.Message:
                    writer.WriteString(this.Text ?? string.Empty);
                    break;
                case ChatToHostCommand.MessageExtra:
                    writer.Write(this.ExtraFlags ?? Array.Empty<byte>());
                    writer.WriteString(this.Text ?? string.Empty);
                    break;
            }
        }

        return stream.ToArray();
    }
}

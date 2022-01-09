// <copyright file="CommandMessage.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// Command message base class.
/// </summary>
internal abstract class CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandMessage"/> class.
    /// </summary>
    /// <param name="memory">Memory region containing message data.</param>
    internal CommandMessage(Memory<byte> memory)
    {
        this.Reader = new BinaryReader(new MemoryStream(memory.ToArray()));
        this.Type = this.Reader.ReadByte();
        this.Id = this.Reader.ReadByte();
        this.Size = this.Reader.ReadUInt16();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandMessage"/> class.
    /// </summary>
    internal CommandMessage()
    {
    }

    /// <summary>
    /// Gets or sets message type (W3GS, GPS or Icecrown).
    /// </summary>
    internal byte Type { get; set; }

    /// <summary>
    /// Gets or sets message id.
    /// </summary>
    internal byte Id { get; set; }

    /// <summary>
    /// Gets or sets message size.
    /// </summary>
    internal ushort Size { get; set; }

    /// <summary>
    /// Gets inner binary reader.
    /// </summary>
    protected BinaryReader? Reader { get; }

    /// <summary>
    /// Converts this message to a byte array.
    /// </summary>
    /// <returns>Returns a new byte array.</returns>
    internal abstract byte[] ToByteArray();

    /// <summary>
    /// Adds header to inner buffer and return a ready to send byte array.
    /// </summary>
    /// <returns>Returns new finalized byte array for this message.</returns>
    internal byte[] FinalByteArray()
    {
        var inner = this.ToByteArray();
        var final = new byte[inner.Length + 4];

        final[0] = GameProtocol.W3GSHeaderConstant;
        final[1] = this.Id;
        final[2] = (byte)((inner.Length + 4) & byte.MaxValue);
        final[3] = (byte)((inner.Length + 4) >> 8);

        Array.Copy(inner, 0, final, 4, inner.Length);

        return final;
    }
}

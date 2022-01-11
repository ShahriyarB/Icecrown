// <copyright file="JoinRequest.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS_REQJOIN
/// Transport Layer:          Transmission Control Protocol (TCP)
/// Application Layer:        Warcraft III In-Game Messages (W3GS)
/// Message Id:               0x1E (30)
/// Direction:                Client to Server
/// Used By:                  Warcraft III Reign of Chaos, Warcraft III The Frozen Throne.
/// </summary>
internal class JoinRequest : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JoinRequest"/> class.
    /// </summary>
    /// <param name="memory">Memory region containing message data.</param>
    public JoinRequest(Memory<byte> memory)
        : base(memory)
    {
        this.HostId = this.Reader.ReadUInt32();
        this.EntryKey = this.Reader.ReadUInt32();
        this.Unknown1 = this.Reader.ReadByte();
        this.ListenPort = this.Reader.ReadUInt16();
        this.PeerKey = this.Reader.ReadUInt32();
        this.Name = this.Reader.ReadNullTeminatedString();
        this.Unknown2 = this.Reader.ReadUInt32();
        this.InternalPort = this.Reader.ReadUInt16();
        this.InternalIp = this.Reader.ReadUInt32();
    }

    /// <summary>
    /// Gets host id (counter).
    /// </summary>
    internal uint HostId { get; }

    /// <summary>
    /// Gets entry key (used in lan).
    /// </summary>
    internal uint EntryKey { get; }

    /// <summary>
    /// Gets Unknown1.
    /// </summary>
    internal byte Unknown1 { get; }

    /// <summary>
    /// Gets listen port.
    /// </summary>
    internal ushort ListenPort { get; }

    /// <summary>
    /// Gets peer key.
    /// </summary>
    internal uint PeerKey { get; }

    /// <summary>
    /// Gets player name.
    /// </summary>
    internal string Name { get; }

    /// <summary>
    /// Gets Unknown2.
    /// </summary>
    internal uint Unknown2 { get; }

    /// <summary>
    /// Gets internal port.
    /// </summary>
    internal ushort InternalPort { get; }

    /// <summary>
    /// Gets internal ip.
    /// </summary>
    internal uint InternalIp { get; }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        using var stream = new MemoryStream(25);
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(this.HostId);
            writer.Write(this.EntryKey);
            writer.Write(this.Unknown1);
            writer.Write(this.ListenPort);
            writer.Write(this.PeerKey);
            writer.WriteString(this.Name);
            writer.Write(this.Unknown2);
            writer.Write(this.InternalPort);
            writer.Write(this.InternalIp);
        }

        return stream.ToArray();
    }
}

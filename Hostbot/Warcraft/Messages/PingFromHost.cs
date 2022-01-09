// <copyright file="PingFromHost.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS ping from host message.
/// </summary>
internal class PingFromHost : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PingFromHost"/> class.
    /// </summary>
    /// <param name="ticks">Ping time in ms.</param>
    public PingFromHost(uint ticks)
    {
        this.Id = GameProtocol.W3GSPingFromHost;
        this.Ticks = ticks;
    }

    /// <summary>
    /// Gets ping ticks.
    /// </summary>
    internal uint Ticks { get; }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        using var stream = new MemoryStream(4);
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(this.Ticks);
        }

        return stream.ToArray();
    }
}

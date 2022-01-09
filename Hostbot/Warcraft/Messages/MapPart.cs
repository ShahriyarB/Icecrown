// <copyright file="MapPart.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

using Ionic.Crc;

/// <summary>
/// W3GS map part message.
/// </summary>
internal class MapPart : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapPart"/> class.
    /// </summary>
    /// <param name="fromPlayerId">From player id.</param>
    /// <param name="toPlayerId">To player id.</param>
    /// <param name="start">Start position.</param>
    /// <param name="data">Full map data.</param>
    public MapPart(byte fromPlayerId, byte toPlayerId, uint start, byte[]? data)
    {
        this.Id = GameProtocol.W3GSMapPart;
        this.FromPlayerId = fromPlayerId;
        this.ToPlayerId = toPlayerId;
        this.Start = start;
        this.Data = data;
    }

    /// <summary>
    /// Gets from player id.
    /// </summary>
    internal byte FromPlayerId { get; }

    /// <summary>
    /// Gets to player id.
    /// </summary>
    internal byte ToPlayerId { get; }

    /// <summary>
    /// Gets start position.
    /// </summary>
    internal uint Start { get; }

    /// <summary>
    /// Gets full map data.
    /// </summary>
    internal byte[]? Data { get; }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        if (this.Data is null)
        {
            return Array.Empty<byte>();
        }

        var end = this.Start + 1442;

        if (end > this.Data.Length)
        {
            end = (uint)this.Data.Length;
        }

        using var stream = new MemoryStream(14 + (int)(end - this.Start));
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(this.ToPlayerId);
            writer.Write(this.FromPlayerId);
            writer.Write(1);
            writer.Write(this.Start);

            using var ms = new MemoryStream(this.Data, (int)this.Start, (int)(end - this.Start));
            writer.Write(new CRC32().GetCrc32(ms));
            writer.Write(ms.ToArray());
        }

        return stream.ToArray();
    }
}

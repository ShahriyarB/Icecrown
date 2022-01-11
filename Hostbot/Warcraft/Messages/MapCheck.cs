// <copyright file="MapCheck.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS_MAPCHECK
/// Transport Layer:          Transmission Control Protocol (TCP)
/// Application Layer:        Warcraft III In-Game Messages (W3GS)
/// Message Id:               0x3D (61)
/// Direction:                Server to Client
/// Used By:                  Warcraft III Reign of Chaos, Warcraft III The Frozen Throne.
/// </summary>
internal class MapCheck : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapCheck"/> class.
    /// </summary>
    /// <param name="mapPath">Map path.</param>
    /// <param name="mapSize">Map size.</param>
    /// <param name="mapInfo">Map info.</param>
    /// <param name="mapCrc">Map crc.</param>
    /// <param name="mapSha1">Map sha1.</param>
    public MapCheck(string mapPath, uint mapSize, uint mapInfo, byte[]? mapCrc, byte[]? mapSha1)
    {
        this.Type = GameProtocol.W3GSHeaderConstant;
        this.Id = GameProtocol.W3GSMapCheck;
        this.MapPath = mapPath;
        this.MapSize = mapSize;
        this.MapInfo = mapInfo;
        this.MapCrc = mapCrc;
        this.MapSha1 = mapSha1;
    }

    /// <summary>
    /// Gets map path.
    /// </summary>
    internal string MapPath { get; }

    /// <summary>
    /// Gets map size.
    /// </summary>
    internal uint MapSize { get; }

    /// <summary>
    /// Gets map info.
    /// CRC of the map file.
    /// </summary>
    internal uint MapInfo { get; }

    /// <summary>
    /// Gets map crc.
    /// </summary>
    internal byte[]? MapCrc { get; }

    /// <summary>
    /// Gets map sha1.
    /// </summary>
    internal byte[]? MapSha1 { get; }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        using var stream = new MemoryStream(36);
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(1);
            writer.WriteString(this.MapPath);
            writer.Write(this.MapSize);
            writer.Write(this.MapInfo);
            writer.Write(this.MapCrc ?? Array.Empty<byte>());
            writer.Write(this.MapSha1 ?? Array.Empty<byte>());
        }

        return stream.ToArray();
    }
}

// <copyright file="GameInfo.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS_GAMEINFO
/// Transport Layer:          Transmission Control Protocol (TCP) and UDP
/// Application Layer:        Warcraft III In-Game Messages (W3GS)
/// Message Id:               0x30 (48)
/// Direction:                Server to Client
/// Used By:                  Warcraft III Reign of Chaos, Warcraft III The Frozen Throne.
/// </summary>
internal class GameInfo : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameInfo"/> class.
    /// </summary>
    /// <param name="warcraftVersion">Warcraft lan version.</param>
    /// <param name="mapGameType">Map game type.</param>
    /// <param name="mapFlags">Map flags.</param>
    /// <param name="mapWidth">Map width.</param>
    /// <param name="mapHeight">Map height.</param>
    /// <param name="gameName">Game name.</param>
    /// <param name="hostName">Host name.</param>
    /// <param name="upTime">Game up time.</param>
    /// <param name="mapPath">Game map path.</param>
    /// <param name="mapCrc">Game map crc.</param>
    /// <param name="slotsTotal">Game total slots.</param>
    /// <param name="slotsOpen">Game open slots.</param>
    /// <param name="port">Game port.</param>
    /// <param name="hostCounter">Game host counter (id).</param>
    /// <param name="entryKey">Game entry key.</param>
    public GameInfo(byte warcraftVersion, MapGameType mapGameType, uint mapFlags, ushort mapWidth, ushort mapHeight, string gameName, string hostName, uint upTime, string mapPath, byte[]? mapCrc, uint slotsTotal, uint slotsOpen, ushort port, uint hostCounter, uint entryKey)
    {
        this.Type = GameProtocol.W3GSHeaderConstant;
        this.Id = GameProtocol.W3GSGameInfo;
        this.WarcraftVersion = warcraftVersion;
        this.MapGameType = mapGameType;
        this.MapFlags = mapFlags;
        this.MapWidth = mapWidth;
        this.MapHeight = mapHeight;
        this.GameName = gameName;
        this.HostName = hostName;
        this.UpTime = upTime;
        this.MapPath = mapPath;
        this.MapCrc = mapCrc;
        this.SlotsTotal = slotsTotal;
        this.SlotsOpen = slotsOpen;
        this.Port = port;
        this.HostCounter = hostCounter;
        this.EntryKey = entryKey;
    }

    /// <summary>
    /// Gets Warcraft lan version.
    /// </summary>
    internal byte WarcraftVersion { get; }

    /// <summary>
    /// Gets map game type.
    /// </summary>
    internal MapGameType MapGameType { get; }

    /// <summary>
    /// Gets map flag.
    /// </summary>
    internal uint MapFlags { get; }

    /// <summary>
    /// Gets map width.
    /// </summary>
    internal ushort MapWidth { get; }

    /// <summary>
    /// Gets map height.
    /// </summary>
    internal ushort MapHeight { get; }

    /// <summary>
    /// Gets game name.
    /// </summary>
    internal string GameName { get; }

    /// <summary>
    /// Gets host name.
    /// </summary>
    internal string HostName { get; }

    /// <summary>
    /// Gets up time.
    /// </summary>
    internal uint UpTime { get; }

    /// <summary>
    /// Gets map path.
    /// </summary>
    internal string MapPath { get; }

    /// <summary>
    /// Gets map crc.
    /// </summary>
    internal byte[]? MapCrc { get; }

    /// <summary>
    /// Gets total slots.
    /// </summary>
    internal uint SlotsTotal { get; }

    /// <summary>
    /// Gets open slots.
    /// </summary>
    internal uint SlotsOpen { get; }

    /// <summary>
    /// Gets game port.
    /// </summary>
    internal ushort Port { get; }

    /// <summary>
    /// Gets host counter (id).
    /// </summary>
    internal uint HostCounter { get; }

    /// <summary>
    /// Gets entry key.
    /// </summary>
    internal uint EntryKey { get; }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        // Make stat string first
        using var statStream = new MemoryStream(14);
        using (var statWriter = new BinaryWriter(statStream))
        {
            statWriter.Write(this.MapFlags);
            statWriter.Write((byte)0);
            statWriter.Write(this.MapWidth);
            statWriter.Write(this.MapHeight);

            if (this.MapCrc is null)
            {
                statWriter.Write(0);
            }
            else
            {
                statWriter.Write(this.MapCrc);
            }
            
            statWriter.WriteString(this.MapPath);
            statWriter.WriteString(this.HostName);
            statWriter.Write((byte)0);
        }

        using var stream = new MemoryStream(36);
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(new byte[] { 80, 88, 51, 87, this.WarcraftVersion, 0, 0, 0 });
            writer.Write(this.HostCounter);
            writer.Write(this.EntryKey);
            writer.WriteString(this.GameName);
            writer.Write((byte)0);
            writer.Write(Utility.EncodeStatString(statStream.ToArray()));
            writer.Write((byte)0);
            writer.Write(this.SlotsTotal);
            writer.Write((uint)this.MapGameType);
            writer.Write(1);
            writer.Write(this.SlotsOpen);
            writer.Write(this.UpTime);
            writer.Write(this.Port);
        }

        return stream.ToArray();
    }
}

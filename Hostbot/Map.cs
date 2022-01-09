// <copyright file="Map.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot
{
    using System.Security.Cryptography;
    using Ionic.Crc;

    /// <summary>
    /// This class contains information about a running game's map.
    /// </summary>
    internal class Map
    {
        private readonly Hostbot hostbot;

        /// <summary>
        /// Initializes a new instance of the <see cref="Map"/> class.
        /// </summary>
        /// <param name="hostbot">Hostbot instance.</param>
        /// <param name="name">Map file name with extension.</param>
        /// <exception cref="FileNotFoundException">Throws if map file not found.</exception>
        internal Map(Hostbot hostbot, string name)
        {
            if (!File.Exists(name))
            {
                throw new FileNotFoundException("Map file not found" + Directory.GetCurrentDirectory(), name);
            }

            this.hostbot = hostbot;
            this.MapPath = name;
            this.LoadInformation();
        }

        /// <summary>
        /// Gets map path.
        /// </summary>
        internal string MapPath { get; }

        /// <summary>
        /// Gets map slots.
        /// </summary>
        internal List<IGameSlot> Slots { get; } = new(GameSlot.MaxSlots);

        /// <summary>
        /// Gets map players count.
        /// </summary>
        internal byte MapNumPlayers { get; private set; }

        /// <summary>
        /// Gets map teams count.
        /// </summary>
        internal uint MapNumTeams { get; private set; }

        /// <summary>
        /// Gets map width.
        /// </summary>
        internal ushort MapWidth { get; private set; }

        /// <summary>
        /// Gets map height.
        /// </summary>
        internal ushort MapHeight { get; private set; }

        /// <summary>
        /// Gets map filter type.
        /// </summary>
        internal byte FilterType { get; private set; }

        /// <summary>
        /// Gets map speed.
        /// </summary>
        internal MapSpeed MapSpeed { get; } = MapSpeed.Fast;

        /// <summary>
        /// Gets map visibility.
        /// </summary>
        internal MapVisibility MapVisibility { get; } = MapVisibility.Default;

        /// <summary>
        /// Gets map observers.
        /// </summary>
        internal MapObservers MapObservers { get; } = MapObservers.None;

        /// <summary>
        /// Gets map flags.
        /// </summary>
        internal MapFlags MapFlags { get; } = MapFlags.TeamsTogether | MapFlags.FixedTeams;

        /// <summary>
        /// Gets map filter maker.
        /// </summary>
        internal MapFilterMaker MapFilterMaker { get; } = MapFilterMaker.Blizzard;

        /// <summary>
        /// Gets map filter size.
        /// </summary>
        internal MapFilterSize MapFilterSize { get; } = MapFilterSize.Large;

        /// <summary>
        /// Gets map filter observers.
        /// </summary>
        internal MapFilterObservers MapFilterObservers { get; } = MapFilterObservers.None;

        /// <summary>
        /// Gets map options.
        /// </summary>
        internal MapOptions MapOptions { get; private set; }

        /// <summary>
        /// Gets map size.
        /// </summary>
        internal uint MapSize { get; private set; }

        /// <summary>
        /// Gets map info (crc).
        /// This is the real CRC.
        /// </summary>
        internal uint MapInfo { get; private set; }

        /// <summary>
        /// Gets map crc.
        /// This is not the real CRC, it's the "xoro" value.
        /// </summary>
        internal byte[]? MapCrc { get; private set; }

        /// <summary>
        /// Gets map sha1.
        /// </summary>
        internal byte[]? MapSha1 { get; private set; }

        /// <summary>
        /// Gets map game flags.
        /// </summary>
        internal uint MapGameFlags
        {
            get
            {
                // Speed
                uint flags = this.MapSpeed switch
                {
                    MapSpeed.Slow => 0x00000000U,
                    MapSpeed.Normal => 0x00000001U,
                    _ => 0x00000002U,
                };

                // Visibility
                flags |= this.MapVisibility switch
                {
                    MapVisibility.HideTerrain => 0x00000100U,
                    MapVisibility.Explored => 0x00000200U,
                    MapVisibility.AlwaysVisible => 0x00000400U,
                    _ => 0x00000800U,
                };

                // Observers
                switch (this.MapObservers)
                {
                    case MapObservers.OnDefeat:
                        flags |= 0x00002000U;
                        break;
                    case MapObservers.Allowed:
                        flags |= 0x00003000U;
                        break;
                    case MapObservers.Referees:
                        flags |= 0x40000000U;
                        break;
                }

                // Teams, units, hero and race
                if ((this.MapFlags & MapFlags.TeamsTogether) == MapFlags.TeamsTogether)
                {
                    flags |= 0x00004000U;
                }

                if ((this.MapFlags & MapFlags.FixedTeams) == MapFlags.FixedTeams)
                {
                    flags |= 0x00060000U;
                }

                if ((this.MapFlags & MapFlags.ShareUnits) == MapFlags.ShareUnits)
                {
                    flags |= 0x01000000U;
                }

                if ((this.MapFlags & MapFlags.RandomHero) == MapFlags.RandomHero)
                {
                    flags |= 0x02000000U;
                }

                if ((this.MapFlags & MapFlags.RandomRaces) == MapFlags.RandomRaces)
                {
                    flags |= 0x04000000U;
                }

                return flags;
            }
        }

        /// <summary>
        /// Gets map game type.
        /// </summary>
        internal MapGameType GameType
        {
            get
            {
                MapGameType type = 0;

                // Maker
                if ((this.MapFilterMaker & MapFilterMaker.User) == MapFilterMaker.User)
                {
                    type |= MapGameType.MakerUser;
                }

                if ((this.MapFilterMaker & MapFilterMaker.Blizzard) == MapFilterMaker.Blizzard)
                {
                    type |= MapGameType.MakerBlizzard;
                }

                // Type
                if ((this.FilterType & (uint)MapFilterType.Melee) == (uint)MapFilterType.Melee)
                {
                    type |= MapGameType.Melee;
                }

                if ((this.FilterType & (uint)MapFilterType.Scenario) == (uint)MapFilterType.Scenario)
                {
                    type |= MapGameType.TypeScenario;
                }

                // Size
                if ((this.MapFilterSize & MapFilterSize.Small) == MapFilterSize.Small)
                {
                    type |= MapGameType.SizeSmall;
                }

                if ((this.MapFilterSize & MapFilterSize.Medium) == MapFilterSize.Medium)
                {
                    type |= MapGameType.SizeMedium;
                }

                if ((this.MapFilterSize & MapFilterSize.Large) == MapFilterSize.Large)
                {
                    type |= MapGameType.SizeLarge;
                }

                // Observers
                if ((this.MapFilterObservers & MapFilterObservers.Full) == MapFilterObservers.Full)
                {
                    type |= MapGameType.ObsFull;
                }

                if ((this.MapFilterObservers & MapFilterObservers.OnDeath) == MapFilterObservers.OnDeath)
                {
                    type |= MapGameType.ObsOnDeath;
                }

                if ((this.MapFilterObservers & MapFilterObservers.None) == MapFilterObservers.None)
                {
                    type |= MapGameType.ObsNone;
                }

                return type;
            }
        }

        /// <summary>
        /// Gets map layout style.
        /// </summary>
        internal byte LayoutStyle
        {
            get
            {
                if ((this.MapOptions & MapOptions.CustomForces) == 0)
                {
                    return 0;
                }

                if ((this.MapOptions & MapOptions.FixedPlayerSettings) == 0)
                {
                    return 1;
                }

                return 3;
            }
        }

        /// <summary>
        /// Gets map file data as byte array.
        /// Used for sending map to players.
        /// </summary>
        internal byte[]? Data { get; private set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            var size = BitConverter.GetBytes(this.MapSize);
            var info = BitConverter.GetBytes(this.MapInfo);

            string ret = "\r\nSize:";

            foreach (var b in size)
            {
                ret += $" {b}";
            }

            ret += "\r\nInfo:";

            foreach (var b in info)
            {
                ret += $" {b}";
            }

            ret += "\r\nCrc:";

            foreach (var b in this.MapCrc)
            {
                ret += $" {b}";
            }

            ret += "\r\nSha1:";

            foreach (var b in this.MapSha1)
            {
                ret += $" {b}";
            }

            return ret;
        }

        /// <summary>
        /// Loads all required map data from it.
        /// </summary>
        /// <exception cref="Exception">Throws if file can't be read as an mpq archive.</exception>
        /// <exception cref="FileNotFoundException">Throws if map file not found.</exception>
        private void LoadInformation()
        {
            byte[] commonJData, blizzardJData;

            // Cache this to use for map downloads
            this.Data = File.ReadAllBytes(this.MapPath);

            using (MemoryStream memoryStream = new(this.Data))
            {
                this.MapSize = (uint)memoryStream.Length;
                this.MapInfo = (uint)new CRC32().GetCrc32(memoryStream);

                using var mpq = new MpqArchive(memoryStream, true);
                if (mpq is null)
                {
                    throw new Exception("Failed to open the map file as an mpq archive");
                }

                // Try to look for "common.j" and "blizzard.j" in map mpq file first
                // If not found revert back to original files in data directory.

                // "common.j"
                if (mpq.FileExists("Scripts\\common.j"))
                {
                    using var common = mpq.OpenFile("Scripts\\common.j");
                    using var stream = new MemoryStream();
                    common.CopyTo(stream);
                    commonJData = stream.ToArray();
                }
                else
                {
                    var commonPath = $"{Settings.Current.DataPath}\\common.j";

                    if (!File.Exists(commonPath))
                    {
                        throw new FileNotFoundException("Original 'common.j' file not found in data directory.");
                    }

                    commonJData = File.ReadAllBytes(commonPath);
                }

                // "blizzard.j"
                if (mpq.FileExists("Scripts\\blizzard.j"))
                {
                    using var blizzard = mpq.OpenFile("Scripts\\blizzard.j");
                    using var stream = new MemoryStream();
                    blizzard.CopyTo(stream);
                    blizzardJData = stream.ToArray();
                }
                else
                {
                    var blizzardPath = $"{Settings.Current.DataPath}\\blizzard.j";

                    if (!File.Exists(blizzardPath))
                    {
                        throw new FileNotFoundException("Original 'blizzard.j' file not found in data directory.");
                    }

                    blizzardJData = File.ReadAllBytes(blizzardPath);
                }

                var foundScript = false;
                var magicBytes = new byte[] { 0x9e, 0x37, 0xf1, 0x03 };
                uint crcValue = 0;
                crcValue ^= Utility.XORRotateLeft(commonJData);
                crcValue ^= Utility.XORRotateLeft(blizzardJData);

                SHA1 sha = SHA1.Create();

                sha.TransformBlock(commonJData, 0, commonJData.Length, commonJData, 0);
                sha.TransformBlock(blizzardJData, 0, blizzardJData.Length, blizzardJData, 0);

                crcValue = Utility.ROTL(crcValue, 3);
                crcValue = Utility.ROTL(crcValue ^ 0x03F1379E, 3);

                sha.TransformBlock(magicBytes, 0, magicBytes.Length, magicBytes, 0);

                string[] scripts = { "war3map.j", "scripts\\war3map.j", "war3map.w3e", "war3map.wpm", "war3map.doo", "war3map.w3u", "war3map.w3b", "war3map.w3d", "war3map.w3a", "war3map.w3q" };

                foreach (string script in scripts)
                {
                    if (foundScript && script == scripts[1])
                    {
                        continue;
                    }

                    if (!mpq.FileExists(script))
                    {
                        continue;
                    }

                    if (script == scripts[0] || script == scripts[1])
                    {
                        foundScript = true;
                    }

                    using var stream = mpq.OpenFile(script);
                    using var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    var fdata = ms.ToArray();

                    crcValue = Utility.ROTL(crcValue ^ Utility.XORRotateLeft(fdata), 3);
                    sha.TransformBlock(fdata, 0, fdata.Length, fdata, 0);
                }

                sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

                this.MapCrc = BitConverter.GetBytes(crcValue);

                if (sha.Hash is null)
                {
                    throw new Exception("Failed to calculate sha1 of the map file.");
                }

                this.MapSha1 = sha.Hash;

                if (!foundScript)
                {
                    Log.Warning("[MAP] couldn't find war3map.j or scripts\\war3map.j in MPQ file, calculated map_crc/sha1 is probably wrong");
                }

                // Finished checking map scripts, crc and sha.
                // Now we setup map fields.
                int closedSlots = 0;
                uint rawMapFlags;
                using (var stream = mpq.OpenFile("war3map.w3i"))
                {
                    if (stream is null)
                    {
                        throw new Exception("Couldn't find war3map.w3i in map mpq file.");
                    }

                    using BinaryReader reader = new(stream);

                    var fileFormat = reader.ReadInt32();
                    if (fileFormat != 18 && fileFormat != 25)
                    {
                        throw new Exception("Unknown w3i file format " + fileFormat);
                    }

                    _ = reader.ReadInt32(); // numSaves
                    _ = reader.ReadInt32(); // editorVersion
                    string mapName = reader.ReadNullTeminatedString();
                    string mapAuthor = reader.ReadNullTeminatedString();
                    string mapDescription = reader.ReadNullTeminatedString();
                    string mapRecommendedPlayers = reader.ReadNullTeminatedString();
                    _ = reader.ReadBytes(32); // cameraBounds
                    _ = reader.ReadBytes(16); // cameraBoundsComplements
                    this.MapWidth = (ushort)reader.ReadInt32();
                    this.MapHeight = (ushort)reader.ReadInt32();
                    rawMapFlags = reader.ReadUInt32();
                    _ = reader.ReadByte(); // groundType

                    if (fileFormat == 25)
                    {
                        _ = reader.ReadInt32(); // loadingScreenBackgroundId
                        _ = reader.ReadNullTeminatedString(); // loadingScreenPath
                    }
                    else
                    {
                        _ = reader.ReadInt32(); // campaignBackgroundId
                    }

                    _ = reader.ReadNullTeminatedString(); // loadingText
                    _ = reader.ReadNullTeminatedString(); // loadingTitle
                    _ = reader.ReadNullTeminatedString(); // loadingSubtitle

                    if (fileFormat == 25)
                    {
                        _ = reader.ReadInt32(); // gameDataSet
                        _ = reader.ReadNullTeminatedString(); // prologueScreenPath
                    }
                    else
                    {
                        _ = reader.ReadInt32(); // loadingScreenId
                    }

                    _ = reader.ReadNullTeminatedString(); // prologueText
                    _ = reader.ReadNullTeminatedString(); // prologueTitle
                    _ = reader.ReadNullTeminatedString(); // prologueSubtitle

                    if (fileFormat == 25)
                    {
                        _ = reader.ReadInt32();         // terrainFog
                        _ = reader.ReadInt32();         // fogStartZHeight
                        _ = reader.ReadInt32();         // fogEndZHeight
                        _ = reader.ReadInt32();         // fogDensity
                        _ = reader.Read();              // fogR
                        _ = reader.Read();              // fogG
                        _ = reader.Read();              // fogB
                        _ = reader.Read();              // fogA
                        _ = reader.ReadInt32();         // globalWeatherId
                        _ = reader.ReadNullTeminatedString(); // customSoundEnvironment
                        _ = reader.Read();              // lightTiseletId
                        _ = reader.Read();              // waterTintR
                        _ = reader.Read();              // waterTintG
                        _ = reader.Read();              // waterTintB
                        _ = reader.Read();              // waterTintA
                    }

                    var mapRawNumPlayers = reader.ReadInt32();

                    for (byte i = 0; i < (byte)mapRawNumPlayers; i++)
                    {
                        GameSlot slot = new(this.hostbot, i, null, 255, SlotStatus.Open, 0, 0, 1, SlotRace.Random);

                        slot.InternalColor = (byte)reader.ReadUInt32();

                        // status
                        switch (reader.ReadUInt32())
                        {
                            case 1:
                                slot.SlotStatus = SlotStatus.Open;
                                break;
                            case 2:
                                slot.SlotStatus = SlotStatus.Occupied;
                                slot.Computer = 1;
                                slot.ComputerType = SlotComputerType.Normal;
                                break;
                            default:
                                slot.SlotStatus = SlotStatus.Closed;
                                closedSlots++;
                                break;
                        }

                        // race
                        slot.Race = reader.ReadInt32() switch
                        {
                            1 => SlotRace.Human,
                            2 => SlotRace.Orc,
                            3 => SlotRace.Undead,
                            4 => SlotRace.Nightelf,
                            _ => SlotRace.Random,
                        };

                        _ = reader.ReadInt32(); // fixedStartPos
                        _ = reader.ReadNullTeminatedString(); // playerName
                        _ = reader.ReadInt32(); // startPosX
                        _ = reader.ReadInt32(); // startPosY
                        _ = reader.ReadInt32(); // allyLowPrio
                        _ = reader.ReadInt32(); // allyHighPrio

                        if (slot.SlotStatus != SlotStatus.Closed)
                        {
                            this.Slots.Add(slot);
                        }
                    }

                    this.MapNumPlayers = (byte)(mapRawNumPlayers - closedSlots);
                    this.MapNumTeams = reader.ReadUInt32();

                    for (byte i = 0; i < this.MapNumTeams; ++i)
                    {
                        int teamFlags = reader.ReadInt32();
                        int playerMask = reader.ReadInt32();

                        for (byte j = 0; j < GameSlot.MaxSlots; ++j)
                        {
                            if ((playerMask & 1) == 1)
                            {
                                foreach (var slot in this.Slots)
                                {
                                    if (slot.Color == j)
                                    {
                                        slot.Team = i;
                                    }
                                }
                            }

                            playerMask >>= 1;
                        }

                        _ = reader.ReadNullTeminatedString(); // teamName
                    }
                }

                this.MapOptions = (MapOptions)rawMapFlags & (MapOptions.Melee | MapOptions.FixedPlayerSettings | MapOptions.CustomForces);
                this.FilterType = (byte)MapFilterType.Scenario;

                if ((this.MapOptions & MapOptions.Melee) != 0)
                {
                    byte team = 0;
                    foreach (var slot in this.Slots)
                    {
                        slot.Team = team++;
                        slot.Race = SlotRace.Random;
                    }

                    this.FilterType = (byte)MapFilterType.Melee;
                }

                if ((this.MapOptions & MapOptions.FixedPlayerSettings) == 0)
                {
                    foreach (var slot in this.Slots)
                    {
                        slot.Race |= SlotRace.Selectable;
                    }
                }
            }

            if ((this.MapFlags & MapFlags.RandomRaces) == MapFlags.RandomRaces)
            {
                foreach (var slot in this.Slots)
                {
                    slot.Race = SlotRace.Random;
                }
            }

            if (this.MapObservers == MapObservers.Allowed || this.MapObservers == MapObservers.Referees)
            {
                while (this.Slots.Count < GameSlot.MaxSlots)
                {
                    this.Slots.Add(new GameSlot(this.hostbot, (byte)this.Slots.Count, null, 255, SlotStatus.Open, 0, GameSlot.MaxSlots, GameSlot.MaxSlots, SlotRace.Random));
                }
            }
        }
    }
}

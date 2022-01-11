// <copyright file="Hostbot.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot
{
    /// <summary>
    /// This class hosts a single match.
    /// </summary>
    public class Hostbot : IHostbot
    {
        private readonly Random random = new();
        private readonly Thread hostThread;
        private readonly TcpListener tcpListener;
        private readonly UdpClient udpClient = new() { EnableBroadcast = true };
        private readonly GameProtocol protocol = new();
        private readonly IPEndPoint broadcastAddr = new(IPAddress.Broadcast, 6112);
        private readonly Queue<OutgoingAction> actions = new();
        private readonly uint hostCounter;
        private readonly uint entryKey;
        private readonly string virtualHostName;
        private readonly ushort hostPort;
        private readonly long randomSeed = Utility.GetTicks();
        private readonly long creationTime = Utility.GetTime();

        private byte virtualHostPlayerId = byte.MaxValue;
        private bool running = true;
        private bool countdownStarted;
        private byte countdownCounter;
        private bool slotInfoChanged;
        private long startedLoadingTicks;
        private long lastActionSentTicks;
        private long lastDownloadCounterResetTicks;
        private long lastCountdownTicks;
        private long lastDownloadTicks;
        private long lastPingTime;
        private long lastLagScreenResetTime;
        private uint downloadCounter;
        private uint syncCounter;
        private GameState gameState;

        /// <summary>
        /// Initializes a new instance of the <see cref="Hostbot"/> class.
        /// </summary>
        /// <param name="mapName">Map name with extension.</param>
        /// <param name="mapHcl">Map HCL.</param>
        /// <param name="gameName">Game name.</param>
        /// <param name="virtualHostName">Virtual host name.</param>
        /// <param name="hostCounter">Host counter (id).</param>
        /// <param name="hostPort">The port to host games on.</param>
        /// <param name="configId">The configuration id used to create this bot.</param>
        internal Hostbot(string mapName, string mapHcl, string gameName, string virtualHostName, uint hostCounter, ushort hostPort, int configId)
        {
            this.Map = new Map(this, mapName);
            this.hostThread = new Thread(this.Update);
            this.tcpListener = new TcpListener(IPAddress.Any, hostPort);
            this.Slots = this.Map.Slots;
            this.ConfigId = configId;
            this.entryKey = (uint)this.random.Next(int.MaxValue);
            this.MapHCL = mapHcl;
            this.GameName = gameName;
            this.virtualHostName = virtualHostName;
            this.hostCounter = hostCounter;
            this.hostPort = hostPort;

            // Register internal events
            this.protocol.OnJoinRequest += this.GameProtocol_OnJoinRequest;
            this.protocol.OnLeaveRequest += this.GameProtocol_OnLeaveRequest;
            this.protocol.OnMapSize += this.GameProtocol_OnMapSize;
            this.protocol.OnChatToHost += this.GameProtocol_OnChatToHost;
            this.protocol.OnPlayerDeleted += this.GameProtocol_OnPlayerDeleted;
            this.protocol.OnPlayerChangeTeam += this.GameProtocol_OnPlayerChangeTeam;
            this.protocol.OnPlayerChangeColor += this.GameProtocol_OnPlayerChangeColor;
            this.protocol.OnPlayerChangeRace += this.GameProtocol_OnPlayerChangeRace;
            this.protocol.OnPlayerChangeHandicap += this.GameProtocol_OnPlayerChangeHandicap;
            this.protocol.OnPlayerLoaded += this.GameProtocol_OnPlayerLoaded;
            this.protocol.OnPlayerOutgoingAction += this.GameProtocol_OnPlayerOutgoingAction;
            this.protocol.OnPlayerOutgoingKeepAlive += this.GameProtocol_OnPlayerOutgoingKeepAlive;

            // Start tcp server thread
            this.tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            this.tcpListener.Start();

            // Start host's thread
            this.hostThread.Start();

            Log.Information($"Hostbot #{this.hostCounter} is running");
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Hostbot"/> class.
        /// </summary>
        ~Hostbot()
        {
            this.Close();
        }

        /// <summary>
        /// Gets current game state.
        /// </summary>
        public GameState GameState => this.gameState;

        /// <inheritdoc/>
        public ushort Latency { get; set; } = Settings.Current.Latency;

        /// <summary>
        /// Gets players list.
        /// </summary>
        public List<IPlayer> Players { get; } = new();

        /// <inheritdoc/>
        public List<IGameSlot> Slots { get; }

        /// <inheritdoc/>
        public string MapHCL { get; set; }

        /// <inheritdoc/>
        public bool IsLagging { get; }

        /// <summary>
        /// Gets this game's map.
        /// </summary>
        internal Map Map { get; }

        /// <summary>
        /// Gets configuration id used to create this bot.
        /// </summary>
        internal int ConfigId { get; }

        /// <summary>
        /// Gets game name.
        /// </summary>
        internal string GameName { get; }

        /// <summary>
        /// Gets current game info message.
        /// </summary>
        /// <remarks>
        /// We use 24 for SlotsTotal because this determines how many PID's Warcraft 3 allocates
        /// We need to make sure Warcraft 3 allocates at least SlotsTotal + 1 but at most 24 PID's
        /// This is because we need an extra PID for the virtual host player (but we always delete the virtual host player when the 24th person joins)
        /// However, we can't send 25 for SlotsTotal because this causes Warcraft 3 to crash when sharing control of units
        /// Nor can we send SlotsTotal because then Warcraft 3 crashes when playing maps with less than 24 PID's (because of the virtual host player taking an extra PID)
        /// We also send 24 for SlotsOpen because Warcraft 3 assumes there's always at least one player in the game (the host)
        /// So if we try to send accurate numbers it'll always be off by one and results in Warcraft 3 assuming the game is full when it still needs one more player
        /// The easiest solution is to simply send 24 for both so the game will always show up as (1/24) players.
        /// </remarks>
        internal GameInfo GameInfo
        {
            get
            {
                return new GameInfo(Settings.Current.WarcraftVersion, this.Map.GameType, this.Map.MapGameFlags, this.Map.MapWidth, this.Map.MapHeight, this.GameName, this.virtualHostName, (uint)(Utility.GetTime() - this.creationTime), this.Map.MapPath, this.Map.MapCrc, GameSlot.MaxSlots, GameSlot.MaxSlots, this.hostPort, this.hostCounter, this.entryKey);
            }
        }

        /// <inheritdoc/>
        public bool StartCountdown()
        {
            if (this.countdownStarted)
            {
                return false;
            }

            if (this.Slots.Any(s => s.SlotStatus == SlotStatus.Occupied && s.Computer == 0 && s.DownloadStatus != 100))
            {
                this.SendAllChat("Can't start the game, not all players have downloaded the map.");
                return false;
            }

            if (this.MapHCL.Length > this.Slots.Count(s => s.SlotStatus == SlotStatus.Occupied))
            {
                Log.Warning("Map HCL is too long, clearing it's value.");
                this.MapHCL = string.Empty;
            }

            this.countdownStarted = true;
            this.countdownCounter = 5;

            return true;
        }

        /// <inheritdoc/>
        public void StopCountdown()
        {
            if (this.countdownStarted && this.gameState == GameState.Lobby)
            {
                this.countdownStarted = false;
                this.countdownCounter = 5;
                this.SendAllChat("Countdown aborted!");
            }
        }

        /// <inheritdoc/>
        public void InsertDummy(bool fillRemaining)
        {
            if (this.GameState != GameState.Lobby)
            {
                return;
            }

            if (fillRemaining)
            {
                foreach (var slot in this.Slots.Where(s => s.SlotStatus == SlotStatus.Open))
                {
                    var playerId = this.FindNewPlayerId();
                    var player = new Player(playerId, $"Dummy [{playerId}]", slot);

                    slot.Player = player;
                    slot.DownloadStatus = 100;
                    slot.SlotStatus = SlotStatus.Occupied;
                    slot.Computer = 0;

                    this.Players.Add(player);
                    this.SendAll(new PlayerInfo(playerId, slot.Player.Name, 0, 0));
                    this.SendSlotsInfo();
                }
            }
            else
            {
                var slot = this.Slots.Find(slot => slot.SlotStatus == SlotStatus.Open);

                if (slot is null)
                {
                    return;
                }

                var playerId = this.FindNewPlayerId();
                var player = new Player(playerId, $"Dummy [{playerId}]", slot);

                slot.Player = player;
                slot.DownloadStatus = 100;
                slot.SlotStatus = SlotStatus.Occupied;
                slot.Computer = 0;

                this.Players.Add(player);
                this.SendAll(new PlayerInfo(playerId, slot.Player.Name, 0, 0));
                this.SendSlotsInfo();

                if (this.countdownStarted)
                {
                    this.StopCountdown();
                }
            }
        }

        /// <inheritdoc/>
        public void RemoveDummy(bool removeAll)
        {
            if (this.GameState != GameState.Lobby)
            {
                return;
            }

            if (!this.Players.Any(p => p.IsDummy))
            {
                return;
            }

            if (removeAll)
            {
                foreach (var dummy in this.Players.Where(p => p.IsDummy))
                {
                    dummy.Slot.Open();
                    dummy.Delete(PlayerLeaveReason.Lobby, "removed by system");
                }
            }
            else
            {
                var dummy = this.Players.FindLast(p => p.IsDummy);

                dummy.Slot.Open();
                dummy.Delete(PlayerLeaveReason.Lobby, "removed by system");
            }

            this.SendSlotsInfo();
        }

        /// <inheritdoc/>
        public void SendAllChat(string message)
        {
            this.SendAllChat(this.GetHostPlayerId(), message);
        }

        /// <summary>
        /// Close this game and it's sockets.
        /// </summary>
        internal void Close()
        {
            try
            {
                this.running = false;
                this.hostThread?.Join();

                foreach (var player in this.Players)
                {
                    ((Player)player).Close();
                }

                this.tcpListener?.Stop();
                this.udpClient.Close();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Gets id of player to be considered the host.
        /// Mainly used for sending map to players and sending chat messages.
        /// </summary>
        /// <returns>Returns host's player id.</returns>
        internal byte GetHostPlayerId()
        {
            // Return virtual host pid if possible.
            if (this.virtualHostPlayerId != byte.MaxValue)
            {
                return this.virtualHostPlayerId;
            }

            // Return first available player.
            foreach (var player in this.Players.Where(p => !p.ShouldDelete))
            {
                return player.Id;
            }

            return byte.MaxValue;
        }

        /// <summary>
        /// Send slots info to all players.
        /// </summary>
        internal void SendSlotsInfo()
        {
            if (this.GameState != GameState.Lobby)
            {
                return;
            }

            this.SendAll(new SlotInfo(this.Slots, (uint)this.randomSeed, this.Map.LayoutStyle, this.Map.MapNumPlayers));
            this.slotInfoChanged = false;
        }

        private byte FindNewPlayerId()
        {
            for (byte i = 1; i < byte.MaxValue; i++)
            {
                if (i == this.virtualHostPlayerId)
                {
                    continue;
                }

                if (!this.Players.Any(p => p.Id == i))
                {
                    return i;
                }
            }

            return byte.MaxValue;
        }

        private byte FindNewColor()
        {
            for (byte i = 0; i < GameSlot.MaxSlots; i++)
            {
                if (this.Slots.Any(s => s.Color == i && s.SlotStatus == SlotStatus.Occupied))
                {
                    continue;
                }

                return i;
            }

            return byte.MaxValue;
        }

        private IGameSlot? FindEmptySlot(byte team, Player player)
        {
            // Find an empty slot based on player's current slot
            var slotIndex = player.Slot.SlotIndex;

            if (slotIndex < this.Slots.Count)
            {
                if (this.Slots[slotIndex].Team != team)
                {
                    // Player is trying to move to another team so start looking from the first slot on that team
                    // We actually just start looking from the very first slot since the next few loops will check the team for us
                    slotIndex = 0;
                }

                // Find an empty slot on the correct team starting from StartSlot
                for (byte i = slotIndex; i < this.Slots.Count; i++)
                {
                    if (this.Slots[i].SlotStatus == SlotStatus.Open && this.Slots[i].Team == team)
                    {
                        return this.Slots[i];
                    }
                }

                // Didn't find an empty slot, but we could have missed one with SID < StartSlot
                // E.g. in the DotA case where I am in slot 4 (yellow), slot 5 (orange) is occupied, and slot 1 (blue) is open and I am trying to move to another slot
                for (byte i = 0; i < slotIndex; i++)
                {
                    if (this.Slots[i].SlotStatus == SlotStatus.Open && this.Slots[i].Team == team)
                    {
                        return this.Slots[i];
                    }
                }
            }

            return null;
        }

        private void CreateVirtualHost()
        {
            if (this.virtualHostPlayerId != byte.MaxValue)
            {
                return;
            }

            this.virtualHostPlayerId = this.FindNewPlayerId();

            this.SendAll(new PlayerInfo(this.virtualHostPlayerId, this.virtualHostName, 0, 0));
        }

        private void DeleteVirtualHost()
        {
            if (this.virtualHostPlayerId == byte.MaxValue)
            {
                return;
            }

            this.SendAll(new PlayerLeft(this.virtualHostPlayerId, PlayerLeaveReason.Lobby));

            this.virtualHostPlayerId = byte.MaxValue;
        }

        private void SendAll(CommandMessage message)
        {
            foreach (var player in this.Players)
            {
                ((Player)player).SendMessage(message);
            }
        }

        private void SendAllChat(byte fromPlayerId, string message)
        {
            if (this.GameState == GameState.Lobby)
            {
                foreach (var part in message.Chunk(byte.MaxValue - 1))
                {
                    this.SendAll(new ChatFromHost(fromPlayerId, this.Players.Select(p => p.Id).ToArray(), ChatToHostCommand.Message, null, null, new string(part)));
                }
            }
            else
            {
                foreach (var part in message.Chunk((byte.MaxValue - 1) / 2))
                {
                    this.SendAll(new ChatFromHost(fromPlayerId, this.Players.Select(p => p.Id).ToArray(), ChatToHostCommand.MessageExtra, null, new byte[] { 0, 0, 0, 0 }, new string(part)));
                }
            }
        }

        private void BroadcastMessage(CommandMessage message)
        {
            this.udpClient.Send(message.FinalByteArray(), this.broadcastAddr);
        }

        private void GameProtocol_OnJoinRequest(Player player, JoinRequest request)
        {
            // Check if new player's name is empty or too long.
            if (string.IsNullOrEmpty(request.Name) || request.Name.Length > 15)
            {
                player.RejectJoin(RejectJoinReason.Full);
                player.Delete(PlayerLeaveReason.Lobby, $"name is empty or too long => {request.Name}");
                return;
            }

            // Check if new player's name is the same as the virtual host name
            if (string.Equals(this.virtualHostName, request.Name))
            {
                player.RejectJoin(RejectJoinReason.Full);
                player.Delete(PlayerLeaveReason.Lobby, "name matches host name");
                return;
            }

            // Check if new player's name is already taken
            var existingPlayer = this.Players.Find(p => string.Equals(p.Name, request.Name, StringComparison.Ordinal));
            if (existingPlayer is not null)
            {
                player.RejectJoin(RejectJoinReason.Full);
                player.Delete(PlayerLeaveReason.Lobby, "another player with this name already exists");
                return;
            }

            // Check if new player has a valid entry key.
            if (request.EntryKey != this.entryKey)
            {
                player.RejectJoin(RejectJoinReason.WrongPassword);
                player.Delete(PlayerLeaveReason.Lobby, "invalid entry key");
                return;
            }

            var slot = (GameSlot?)this.Slots.Find(slot => slot.SlotStatus == SlotStatus.Open);

            if (slot is null)
            {
                player.RejectJoin(RejectJoinReason.Full);
                player.Delete(PlayerLeaveReason.Lobby, "lobby is full");
                return;
            }

            if (this.Players.Count(p => p.IsConnected) >= GameSlot.MaxSlots)
            {
                this.DeleteVirtualHost();
            }

            // Set player information
            player.Id = this.FindNewPlayerId();
            player.Name = request.Name;
            player.InternalIp = request.InternalIp;

            // TODO: Remove this and implement roles.
            if (player.Name == "admin")
            {
                player.Role = Role.Administrator;
            }

            player.Slot = slot;
            slot.Player = player;
            slot.DownloadStatus = byte.MaxValue;
            slot.SlotStatus = SlotStatus.Occupied;
            slot.Computer = 0;

            if ((this.Map.MapOptions & MapOptions.CustomForces) == 0)
            {
                slot.Team = byte.MaxValue;
                slot.InternalColor = byte.MaxValue;
                slot.Race = (this.Map.MapFlags & MapFlags.RandomRaces) == MapFlags.RandomRaces ? SlotRace.Random : SlotRace.Random | SlotRace.Selectable;

                var numOtherPlayers = this.Slots.Count(s => s.SlotStatus == SlotStatus.Occupied && s.Team != byte.MaxValue);

                if (numOtherPlayers < this.Map.MapNumPlayers)
                {
                    if (slot.SlotIndex < this.Map.MapNumPlayers)
                    {
                        slot.Team = slot.SlotIndex;
                    }
                    else
                    {
                        slot.Team = 0;
                    }

                    slot.InternalColor = this.FindNewColor();
                }
            }

            // Send slot info to the new player.
            // The SlotInfoJoin packet also tells the client their assigned PID and that the join was successful.
            player.SendMessage(new SlotInfoJoin(player.Id, player.Port, player.ExternalIp, this.Slots, (uint)this.randomSeed, this.Map.LayoutStyle, this.Map.MapNumPlayers));

            // Send virtual host info and fake player info (if present) to the new player.
            if (this.virtualHostPlayerId != byte.MaxValue)
            {
                player.SendMessage(new PlayerInfo(this.virtualHostPlayerId, this.virtualHostName, 0, 0));
            }

            foreach (var p in this.Players.Where(p => p.Id != player.Id).Cast<Player>())
            {
                // Send info about the new player to every other player.
                p.SendMessage(new PlayerInfo(player.Id, player.Name, player.ExternalIp, player.InternalIp));

                // Send info about every other player to the new player.
                player.SendMessage(new PlayerInfo(p.Id, p.Name, p.ExternalIp, p.InternalIp));
            }

            // Send a map check packet to the new player.
            player.SendMessage(new MapCheck(this.Map.MapPath, this.Map.MapSize, this.Map.MapInfo, this.Map.MapCrc, this.Map.MapSha1));

            // Send slot info to everyone, so the new player gets this info twice but everyone else still needs to know the new slot layout.
            this.SendSlotsInfo();

            // Send welcome message.
            player.SendChat($"Welcome {player.Name}!");
            player.SendChat($"Icecrown hostbot version {Program.Version}");

            if (this.countdownStarted)
            {
                this.StopCountdown();
            }
        }

        private void GameProtocol_OnLeaveRequest(Player player, PlayerLeaveReason reason)
        {
            player.Delete(reason, "has left the game voluntarily");

            if (this.GameState == GameState.Lobby)
            {
                player.Slot?.Open();
            }
        }

        private void GameProtocol_OnMapSize(Player player, MapSize mapSize)
        {
            if (this.GameState != GameState.Lobby)
            {
                return;
            }

            if (mapSize.Flag != 1 || mapSize.SizeInBytes != this.Map.MapSize)
            {
                if (Settings.Current.DownloadLimit >= 0)
                {
                    // The player doesn't have the map
                    if (!player.IsDownloadStarted && mapSize.Flag == 1)
                    {
                        Log.Information($"Map download started for {player.Name}");

                        player.SendMessage(new StartDownload(this.GetHostPlayerId()));
                        player.IsDownloadStarted = true;
                        player.StartedDownloadTicks = Utility.GetTicks();
                    }
                    else
                    {
                        player.LastMapPartAcked = mapSize.SizeInBytes;
                    }
                }
                else
                {
                    player.Delete(PlayerLeaveReason.Lobby, "doesn't have the map and map downloads are disabled by hostbot");
                    player.Slot?.Open();
                }
            }
            else if (player.IsDownloadStarted)
            {
                // Calculate download rate
                var seconds = (Utility.GetTicks() - player.StartedDownloadTicks) / 1000d;
                var rate = this.Map.MapSize / 1024 / seconds;

                Log.Information($"Map download finished for player {player.Name} in {seconds} second(s)");
                this.SendAllChat($"Player {player.Name} downloaded the map in {seconds} seconds ({(int)rate} KB/sec)");

                player.IsDownloadFinished = true;
                player.FinishedDownloadTime = Utility.GetTime();
            }

            var downloadStatus = (byte)((float)mapSize.SizeInBytes / this.Map.MapSize * 100f);

            if (downloadStatus > 100)
            {
                downloadStatus = 100;
            }

            if (player.Slot is not null)
            {
                // Only send the slot info if the download status changed
                if (player.Slot.DownloadStatus != downloadStatus)
                {
                    player.Slot.DownloadStatus = downloadStatus;

                    // We don't actually send the new slot info here
                    // This is an optimization because it's possible for a player to download a map very quickly
                    // If we send a new slot update for every percentage change in their download status it adds up to a lot of data
                    // Instead, we mark the slot info as "out of date" and update it only once in awhile (once per second when this comment was made)
                    this.slotInfoChanged = true;
                }
            }
        }

        private void GameProtocol_OnChatToHost(Player player, ChatToHost chatToHost)
        {
            if (player.Id != chatToHost.FromPlayerId)
            {
                return;
            }

            if (chatToHost.Command is ChatToHostCommand.Message or ChatToHostCommand.MessageExtra)
            {
                if (chatToHost.ExtraFlags?.Length > 0)
                {
                    if (chatToHost.ExtraFlags[0] == 0)
                    {
                        // In game [All] message.
                        Log.Information($"[HB#{this.hostCounter}] [All] [{player.Name}] {chatToHost.Message}");
                    }
                }
                else
                {
                    // Lobby message.
                    Log.Information($"[HB#{this.hostCounter}] [Lobby] [{player.Name}] {chatToHost.Message}");
                }

                if (string.IsNullOrEmpty(chatToHost.Message))
                {
                    return;
                }

                bool relay = true;

                // This text message is a command.
                if (chatToHost.Message[0] == Settings.Current.CommandDelimiter && chatToHost.Message.Length > 1)
                {
                    // Remove command delimiter then split the message.
                    var args = chatToHost.Message[1..].Split(' ');

                    // Don't relay chat message if the command executed successfully.
                    relay = !GameProtocol.PlayerSentCommand(this, player, args);
                }

                // Relay the chat message to other players.
                if (relay)
                {
                    foreach (var p in this.Players.Cast<Player>().Where(p => chatToHost.ToPlayerIds.Contains(p.Id)))
                    {
                        p.SendMessage(new ChatFromHost(chatToHost.FromPlayerId, chatToHost.ToPlayerIds, chatToHost.Command, chatToHost.Arg, chatToHost.ExtraFlags, chatToHost.Message ?? string.Empty));
                    }
                }
            }
            else if (!this.countdownStarted)
            {
                switch (chatToHost.Command)
                {
                    case ChatToHostCommand.ChangeTeam:
                        this.protocol.PlayerChangeTeam(player, chatToHost.Arg ?? default);
                        break;
                    case ChatToHostCommand.ChangeColor:
                        this.protocol.PlayerChangeColor(player, chatToHost.Arg ?? default);
                        break;
                    case ChatToHostCommand.ChangeRace:
                        this.protocol.PlayerChangeRace(player, chatToHost.Arg ?? default);
                        break;
                    case ChatToHostCommand.ChangeHandicap:
                        this.protocol.PlayerChangeHandicap(player, chatToHost.Arg ?? default);
                        break;
                }
            }
        }

        private void GameProtocol_OnPlayerDeleted(Player player)
        {
            Log.Information($"[HB#{this.hostCounter}] Deleting player {player.Name} (reason: {player.LeaveReasonMessage})");

            // Close player socket.
            player.Client?.Close();

            // Tell everyone about the player leaving.
            this.SendAll(new PlayerLeft(player.Id, player.LeaveReasonCode));

            if (this.GameState == GameState.InGame)
            {
                this.SendAllChat($"{player.Name} {player.LeaveReasonMessage}");
            }
            else if (this.GameState == GameState.Lobby)
            {
                if (!player.IsDummy)
                {
                    player.Slot?.Open();
                }

                // Abort the countdown if there was one in progress.
                if (this.countdownStarted)
                {
                    this.StopCountdown();
                }

                this.SendSlotsInfo();
            }
            else if (this.GameState != GameState.Finished)
            {
                // Keep track of this player so we can slap him in the face later.
            }
        }

        private void GameProtocol_OnPlayerChangeTeam(Player player, byte team)
        {
            if ((this.Map.MapOptions & MapOptions.CustomForces) != 0)
            {
                var newSlot = this.FindEmptySlot(team, player);

                if (newSlot is null)
                {
                    return;
                }

                player.Slot.Swap(newSlot);
            }
            else
            {
                if (team > GameSlot.MaxSlots)
                {
                    return;
                }

                if (team == GameSlot.MaxSlots)
                {
                    if (this.Map.MapObservers is not MapObservers.Allowed and not MapObservers.Referees)
                    {
                        return;
                    }
                }
                else
                {
                    if (team >= this.Map.MapNumPlayers)
                    {
                        return;
                    }

                    // Make sure there aren't too many other players already
                    byte numOtherPlayers = 0;

                    foreach (var slot in this.Slots)
                    {
                        if (slot.SlotStatus == SlotStatus.Occupied && slot.Team != GameSlot.MaxSlots && slot.PlayerId != player.Id)
                        {
                            numOtherPlayers++;
                        }
                    }

                    if (numOtherPlayers >= this.Map.MapNumPlayers)
                    {
                        return;
                    }
                }

                if (player.Slot is not null)
                {
                    player.Slot.Team = team;

                    if (team == GameSlot.MaxSlots)
                    {
                        // If they're joining the observer team give them the observer color
                        player.Slot.Color = GameSlot.MaxSlots;
                    }
                    else if (player.Slot.Color == GameSlot.MaxSlots)
                    {
                        // If they're joining a regular team give them an unused color
                        player.Slot.Color = this.FindNewColor();
                    }

                    this.SendSlotsInfo();
                }
            }
        }

        private void GameProtocol_OnPlayerChangeColor(Player player, byte color)
        {
            if ((this.Map.MapOptions & MapOptions.FixedPlayerSettings) != 0)
            {
                return;
            }

            if (color > GameSlot.MaxSlots - 1)
            {
                return;
            }

            if (player.Slot is not null)
            {
                if (player.Slot.Team == GameSlot.MaxSlots)
                {
                    return;
                }

                player.Slot.Color = color;
                this.SendSlotsInfo();
            }
        }

        private void GameProtocol_OnPlayerChangeRace(Player player, byte race)
        {
            if ((this.Map.MapOptions & MapOptions.FixedPlayerSettings) != 0 || (this.Map.MapFlags & MapFlags.RandomRaces) != 0)
            {
                return;
            }

            var eRace = (SlotRace)race;

            if (eRace is not SlotRace.Human and not SlotRace.Orc and not SlotRace.Nightelf and not SlotRace.Undead and not SlotRace.Random)
            {
                return;
            }

            if (player.Slot is not null)
            {
                player.Slot.Race = eRace | SlotRace.Selectable;
                this.SendSlotsInfo();
            }
        }

        private void GameProtocol_OnPlayerChangeHandicap(Player player, byte handicap)
        {
            if ((this.Map.MapOptions & MapOptions.FixedPlayerSettings) != 0)
            {
                return;
            }

            if (handicap is not 50 and not 60 and not 70 and not 80 and not 90 and not 100)
            {
                return;
            }

            if (player.Slot is not null)
            {
                player.Slot.Handicap = handicap;
                this.SendSlotsInfo();
            }
        }

        private void GameProtocol_OnPlayerLoaded(Player player)
        {
            Log.Information($"[HB#{this.hostCounter}] player \"{player.Name}\" finished loading in {(player.FinishedLoadingTicks - this.startedLoadingTicks) / 1000} seconds");

            if (player.Id != byte.MaxValue)
            {
                this.SendAll(new PlayerLoaded(player.Id));
            }
        }

        private void GameProtocol_OnPlayerOutgoingAction(Player player, OutgoingAction outgoingAction)
        {
            this.actions.Enqueue(outgoingAction);

            // Notify players if someone is saving the map.
            if (outgoingAction.Action.Length > 0 && outgoingAction.Action[0] == 6)
            {
                Log.Information($"[HB#{this.hostCounter}] player \"{player.Name}\" is saving the game");
                this.SendAllChat($"Player \"{player.Name}\" is saving the game");
            }

            // Process stats.
            if (outgoingAction.Action.Length >= 6)
            {
                // TODO: Process action.
            }
        }

        private void GameProtocol_OnPlayerOutgoingKeepAlive(Player player, OutgoingKeepAlive outgoingKeepAlive)
        {
            if (this.Players.Any(p => !p.IsDummy && ((Player)p).Checksums.Count == 0))
            {
                return;
            }

            var checksum = player.Checksums.Peek();

            foreach (var pl in this.Players.Where(p => !p.IsDummy))
            {
                if (((Player)pl).Checksums.Peek() != checksum)
                {
                    this.SendAllChat("Desync detected");
                    Log.Warning($"[HB#{this.hostCounter}] Desync detected");
                }
            }

            foreach (var pl in this.Players.Where(p => !p.IsDummy))
            {
                ((Player)pl).Checksums.Dequeue();
            }
        }

        private void EventGameStarted()
        {
            Log.Information($"[HB#{this.hostCounter}] started loading with {this.Players.Count(p => p.IsConnected && !p.IsDummy)} player(s)");

            if (!string.IsNullOrEmpty(this.MapHCL) && this.MapHCL.Length <= this.Slots.Count(s => s.SlotStatus == SlotStatus.Occupied))
            {
                const string hclChars = "abcdefghijklmnopqrstuvwxyz0123456789 -=,.";

                if (this.MapHCL.All(c => hclChars.Contains(c)))
                {
                    byte[] encodingMap = new byte[256];
                    byte j = 0;

                    for (int i = 0; i < encodingMap.Length; i++)
                    {
                        if (j is 0 or 50 or 60 or 70 or 80 or 90 or 100)
                        {
                            j++;
                        }

                        encodingMap[i] = j++;
                    }

                    byte currentSlot = 0;

                    foreach (var ch in this.MapHCL)
                    {
                        while (this.Slots[currentSlot].SlotStatus != SlotStatus.Occupied)
                        {
                            currentSlot++;
                        }

                        byte handicapIndex = (byte)((this.Slots[currentSlot].Handicap - 50) / 10);
                        this.Slots[currentSlot++].Handicap = encodingMap[handicapIndex + (hclChars.IndexOf(ch) * 6)];
                    }

                    this.SendSlotsInfo();
                }
            }

            this.startedLoadingTicks = Utility.GetTicks();
            this.lastLagScreenResetTime = Utility.GetTime();
            this.gameState = GameState.Loading;

            this.SendAll(new CountdownStart());
            this.DeleteVirtualHost();
            this.SendAll(new CountdownEnd());

            foreach (var dummy in this.Players.Where(p => p.IsDummy))
            {
                dummy.FinishedLoadingTicks = this.startedLoadingTicks;
                this.SendAll(new PlayerLoaded(dummy.Id));
            }

            // Close the listening socket.
            this.tcpListener.Stop();
        }

        private void SendAllActions()
        {
            this.syncCounter++;

            // We aren't allowed to send more than 1460 bytes in a single packet but it's possible we might have more than that many bytes waiting in the queue.
            if (this.actions.Count > 0)
            {
                // We use a "sub actions queue" which we keep adding actions to until we reach the size limit
                // Start by adding one action to the sub actions queue
                var subActions = new Queue<OutgoingAction>();
                var action = this.actions.Dequeue();
                var subActionsLength = action.GetLength();

                subActions.Enqueue(action);

                while (this.actions.Count > 0)
                {
                    action = this.actions.Dequeue();

                    // Check if adding the next action to the sub actions queue would put us over the limit (1452 because the INCOMING_ACTION and INCOMING_ACTION2 packets use an extra 8 bytes)
                    if (subActionsLength + action.GetLength() > 1452)
                    {
                        // We'd be over the limit if we added the next action to the sub actions queue
                        // So send everything already in the queue and then clear it out
                        // The W3GS_INCOMING_ACTION2 packet handles the overflow but it must be sent *before* the corresponding W3GS_INCOMING_ACTION packet
                        this.SendAll(new IncomingAction2(subActions.ToArray()));

                        subActions.Clear();
                        subActionsLength = 0;
                    }

                    subActions.Enqueue(action);
                    subActionsLength += action.GetLength();
                }

                this.SendAll(new IncomingAction(subActions.ToArray(), this.Latency));
                subActions.Clear();
            }
            else
            {
                this.SendAll(new IncomingAction(this.actions.ToArray(), this.Latency));
            }
        }

        private void Update()
        {
            while (this.running)
            {
                long time = Utility.GetTime(), ticks = Utility.GetTicks();

                // Send ping message to players every 5 seconds.
                // We also use this timer to broadcast game info to lan.
                if (time - this.lastPingTime >= 5)
                {
                    this.SendAll(new PingFromHost((uint)ticks));

                    if (!this.countdownStarted && Settings.Current.BroadcastLan)
                    {
                        this.BroadcastMessage(this.GameInfo);
                    }

                    this.lastPingTime = time;
                }

                // Game has ended and so are we.
                if (this.GameState == GameState.InGame && this.Players.Count == 0)
                {
                    this.Close();
                    return;
                }

                // Update players.
                // We don't use foreach here because it throws exception if we modify players list during iteration (it might happen in insert dummy function).
                var index = 0;
                while (index < this.Players.Count)
                {
                    var player = (Player)this.Players[index++];

                    player.Update();

                    if (player.ShouldDelete)
                    {
                        this.protocol.PlayerDeleted(player);
                    }
                }

                // Remove deleted players from the list.
                this.Players.RemoveAll(p => p.ShouldDelete);

                // Set game state to InGame if we're in loading and everyone finished loading.
                if (this.GameState == GameState.Loading)
                {
                    if (this.Players.All(p => p.IsLoadingFinished))
                    {
                        this.gameState = GameState.InGame;
                    }
                }

                // Relay game actions.
                if (this.GameState == GameState.InGame && !this.IsLagging && ticks - this.lastActionSentTicks >= this.Latency)
                {
                    this.SendAllActions();
                    this.lastActionSentTicks = ticks;
                }

                // Create the virtual host player.
                if (this.GameState == GameState.Lobby && this.virtualHostPlayerId == byte.MaxValue && this.Players.Count(p => p.IsConnected) < GameSlot.MaxSlots)
                {
                    this.CreateVirtualHost();
                }

                // Send more map data.
                if (this.GameState == GameState.Lobby && ticks - this.lastDownloadCounterResetTicks >= 1000)
                {
                    // Hackhack: another timer hijack is in progress here
                    // Since the download counter is reset once per second it's a great place to update the slot info if necessary
                    if (this.slotInfoChanged)
                    {
                        this.SendSlotsInfo();
                    }

                    this.downloadCounter = 0;
                    this.lastDownloadCounterResetTicks = ticks;
                }

                if (this.GameState == GameState.Lobby && ticks - this.lastDownloadTicks >= 100 && Settings.Current.DownloadLimit >= 0)
                {
                    byte downloaders = 0;

                    foreach (var p in this.Players)
                    {
                        var player = (Player)p;

                        if (player.IsDownloadStarted && !player.IsDownloadFinished)
                        {
                            downloaders++;

                            // Allow maximum of 3 players to download the map simultaneously.
                            if (downloaders > 3)
                            {
                                break;
                            }

                            // Send up to 100 pieces of the map at once so that the download goes faster
                            // If we wait for each MAPPART packet to be acknowledged by the client it'll take a long time to download
                            // This is because we would have to wait the round trip time (the ping time) between sending every 1442 bytes of map data
                            // Doing it this way allows us to send at least 140 KB in each round trip int32_terval which is much more reasonable
                            // The theoretical throughput is [140 KB * 1000 / ping] in KB/sec so someone with 100 ping (round trip ping, not LC ping) could download at 1400 KB/sec
                            // Note: this creates a queue of map data which clogs up the connection when the client is on a slower connection (e.g. dialup)
                            // In this case any changes to the lobby are delayed by the amount of time it takes to send the queued data (i.e. 140 KB, which could be 30 seconds or more)
                            // For example, players joining and leaving, slot changes, chat messages would all appear to happen much later for the low bandwidth player
                            // Note: the throughput is also limited by the number of times this code is executed each second
                            // E.g. if we send the maximum amount (140 KB) 10 times per second the theoretical throughput is 1400 KB/sec
                            // Therefore the maximum throughput is 1400 KB/sec regardless of ping and this value slowly diminishes as the player's ping increases
                            // In addition to this, the throughput is limited by the configuration value bot_maxdownloadspeed
                            // In summary: the actual throughput is MIN( 140 * 1000 / ping, 1400, bot_maxdownloadspeed ) in KB/sec assuming only one player is downloading the map
                            while (player.LastMapPartSent < (player.LastMapPartAcked + (1442 * 100)) && player.LastMapPartSent < this.Map.MapSize)
                            {
                                if (player.LastMapPartSent == 0)
                                {
                                    // Overwrite the "started download ticks" since this is the first time we've sent any map data to the player
                                    // Prior to this we've only determined if the player needs to download the map but it's possible we could have delayed sending any data due to download limits
                                    player.StartedDownloadTicks = ticks;
                                }

                                // Limit the download speed if we're sending too much data
                                // The download counter is the # of map bytes downloaded in the last second (it's reset once per second)
                                if (Settings.Current.DownloadLimit > 0 && this.downloadCounter > Settings.Current.DownloadLimit * 1024)
                                {
                                    break;
                                }

                                player.SendMessage(new MapPart(this.GetHostPlayerId(), player.Id, player.LastMapPartSent, this.Map.Data));
                                player.LastMapPartSent += 1442;
                                this.downloadCounter += 1442;
                            }
                        }
                    }

                    this.lastDownloadTicks = ticks;
                }

                if (this.countdownStarted && ticks - this.lastCountdownTicks >= 1000)
                {
                    if (this.countdownCounter > 0)
                    {
                        this.SendAllChat($"Game starts in {this.countdownCounter--}");
                    }
                    else if (this.GameState == GameState.Lobby)
                    {
                        this.EventGameStarted();
                    }

                    this.lastCountdownTicks = ticks;
                }

                // Send queued messages to players.
                foreach (var player in this.Players)
                {
                    ((Player)player).SendMessages();
                }

                // Accept new tcp connections if we're in lobby and we have at least one pending connection request.
                while (this.GameState == GameState.Lobby && this.tcpListener.Pending())
                {
                    this.Players.Add(new Player(this.tcpListener.AcceptTcpClient(), this.protocol, this));
                }

                Thread.Sleep(1);
            }
        }
    }
}
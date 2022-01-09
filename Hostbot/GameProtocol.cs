// <copyright file="GameProtocol.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft;

/// <summary>
/// Warcraft III Game Protocol.
/// Nothing fancy here, every incoming packet and chat command will be sent here first then to registered events of each message (hostbot -> plugins).
/// </summary>
internal class GameProtocol
{
    /// <summary>
    /// W3GS header constant.
    /// </summary>
    internal const byte W3GSHeaderConstant = 247;

    /// <summary>
    /// W3GS ping from host message id.
    /// </summary>
    internal const byte W3GSPingFromHost = 1;

    /// <summary>
    /// W3GS slot info join id.
    /// </summary>
    internal const byte W3GSSlotInfoJoin = 4;

    /// <summary>
    /// W3GS reject join message id.
    /// </summary>
    internal const byte W3GSRejectJoin = 5;

    /// <summary>
    /// W3GS player info message id.
    /// </summary>
    internal const byte W3GSPlayerInfo = 6;

    /// <summary>
    /// W3GS player leave others message id.
    /// </summary>
    internal const byte W3GSPlayerLeaveOthers = 7;

    /// <summary>
    /// W3GS game loaded others message id.
    /// </summary>
    internal const byte W3GSGameLoadedOthers = 8;

    /// <summary>
    /// W3GS slot info message id.
    /// </summary>
    internal const byte W3GSSlotInfo = 9;

    /// <summary>
    /// W3GS countdown start message id.
    /// </summary>
    internal const byte W3GSCountdownStart = 10;

    /// <summary>
    /// W3GS countdown end message id.
    /// </summary>
    internal const byte W3GSCountdownEnd = 11;

    /// <summary>
    /// W3GS incoming action message id.
    /// </summary>
    internal const byte W3GSIncomingAction = 12;

    /// <summary>
    /// W3GS chat from host message id.
    /// </summary>
    internal const byte W3GSChatFromHost = 15;

    /// <summary>
    /// W3GS join request message id.
    /// </summary>
    internal const byte W3GSJoinRequest = 30;

    /// <summary>
    /// W3GS leave request message id.
    /// </summary>
    internal const byte W3GSLeaveRequest = 33;

    /// <summary>
    /// W3GS game loaded self message id.
    /// </summary>
    internal const byte W3GSGameLoadedSelf = 35;

    /// <summary>
    /// W3GS outgoing action message id.
    /// </summary>
    internal const byte W3GSOutgoingAction = 38;

    /// <summary>
    /// W3GS outgoing keep alive message id.
    /// </summary>
    internal const byte W3GSOutgoingKeepAlive = 39;

    /// <summary>
    /// W3GS chat to host message id.
    /// </summary>
    internal const byte W3GSChatToHost = 40;

    /// <summary>
    /// W3GS game info message id.
    /// </summary>
    internal const byte W3GSGameInfo = 48;

    /// <summary>
    /// W3GS map check message id.
    /// </summary>
    internal const byte W3GSMapCheck = 61;

    /// <summary>
    /// W3GS start download message id.
    /// </summary>
    internal const byte W3GSStartDownload = 63;

    /// <summary>
    /// W3GS map size message id.
    /// </summary>
    internal const byte W3GSMapSize = 66;

    /// <summary>
    /// W3GS map part message id.
    /// </summary>
    internal const byte W3GSMapPart = 67;

    /// <summary>
    /// W3GS pong to host message id.
    /// </summary>
    internal const byte W3GSPongToHost = 70;

    /// <summary>
    /// W3GS incoming action 2 message id.
    /// </summary>
    internal const byte W3GSIncomingAction2 = 72;

    /// <summary>
    /// Application wide registered commands.
    /// </summary>
    internal static readonly Dictionary<string[], ICommand> Commands;

    static GameProtocol()
    {
        Commands = new Dictionary<string[], ICommand>()
        {
            [new[] { "help", "h" }] = new HelpCommand(),
            [new[] { "start", "s" }] = new StartCommand(),
            [new[] { "abort", "a" }] = new AbortCommand(),
            [new[] { "ping", "p" }] = new PingCommand(),
            [new[] { "latency", "l" }] = new LatencyCommand(),
            [new[] { "dummy", "d" }] = new DummyCommand(),
            [new[] { "open", "o" }] = new OpenCommand(),
            [new[] { "close", "c" }] = new CloseCommand(),
            [new[] { "kick", "k" }] = new KickCommand(),
            [new[] { "version", "v" }] = new VersionCommand(),
            [new[] { "swap" }] = new SwapCommand(),
            [new[] { "hcl" }] = new HCLCommand(),
        };
    }

    /// <summary>
    /// Function template for W3GS leave request message event.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="reason">Leave reason.</param>
    internal delegate void W3GSLeaveRequestCallback(Player player, PlayerLeaveReason reason);

    /// <summary>
    /// Function template for W3GS join request message event.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="request">Join request data.</param>
    internal delegate void W3GSJoinRequestCallback(Player player, JoinRequest request);

    /// <summary>
    /// Function template for W3GS map size message event.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="mapSize">Map size data.</param>
    internal delegate void W3GSMapSizeCallback(Player player, MapSize mapSize);

    /// <summary>
    /// Function template for W3GS chat to host message event.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="chatToHost">Message data.</param>
    internal delegate void W3GSChatToHostCallback(Player player, ChatToHost chatToHost);

    /// <summary>
    /// Function template for player removed event.
    /// </summary>
    /// <param name="player">Player instance.</param>
    internal delegate void PlayerDeletedCallback(Player player);

    /// <summary>
    /// Function template for change team event.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="team">Player requested team.</param>
    internal delegate void PlayerChangeTeamCallback(Player player, byte team);

    /// <summary>
    /// Function template for change team event.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="color">Player requested color.</param>
    internal delegate void PlayerChangeColorCallback(Player player, byte color);

    /// <summary>
    /// Function template for change race event.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="race">Player requested race.</param>
    internal delegate void PlayerChangeRaceCallback(Player player, byte race);

    /// <summary>
    /// Function template for change handicap event.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="handicap">Player requested handicap.</param>
    internal delegate void PlayerChangeHandicapCallback(Player player, byte handicap);

    /// <summary>
    /// Function template for player load event.
    /// </summary>
    /// <param name="player">Player instance.</param>
    internal delegate void PlayerLoadedCallback(Player player);

    /// <summary>
    /// Function template for player load event.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="outgoingAction">Player outgoing action.</param>
    internal delegate void PlayerActionCallback(Player player, OutgoingAction outgoingAction);

    /// <summary>
    /// Function template for player keep alive event.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="outgoingKeepAlive">Player outgoing keep alive.</param>
    internal delegate void PlayerKeepAliveCallback(Player player, OutgoingKeepAlive outgoingKeepAlive);

    /// <summary>
    /// Event for W3GS leave request message.
    /// </summary>
    internal event W3GSLeaveRequestCallback? OnLeaveRequest;

    /// <summary>
    /// Event for W3GS join request message.
    /// </summary>
    internal event W3GSJoinRequestCallback? OnJoinRequest;

    /// <summary>
    /// Event for W3GS map size message.
    /// </summary>
    internal event W3GSMapSizeCallback? OnMapSize;

    /// <summary>
    /// Event for W3GS chat to host message.
    /// </summary>
    internal event W3GSChatToHostCallback? OnChatToHost;

    /// <summary>
    /// Event for player delete.
    /// </summary>
    internal event PlayerDeletedCallback? OnPlayerDeleted;

    /// <summary>
    /// Event for player change team.
    /// </summary>
    internal event PlayerChangeTeamCallback? OnPlayerChangeTeam;

    /// <summary>
    /// Event for player change color.
    /// </summary>
    internal event PlayerChangeColorCallback? OnPlayerChangeColor;

    /// <summary>
    /// Event for player change race.
    /// </summary>
    internal event PlayerChangeRaceCallback? OnPlayerChangeRace;

    /// <summary>
    /// Event for player change handicap.
    /// </summary>
    internal event PlayerChangeHandicapCallback? OnPlayerChangeHandicap;

    /// <summary>
    /// Event for player game load.
    /// </summary>
    internal event PlayerLoadedCallback? OnPlayerLoaded;

    /// <summary>
    /// Event for player outgoing action.
    /// </summary>
    internal event PlayerActionCallback? OnPlayerOutgoingAction;

    /// <summary>
    /// Event for player outgoing keep alive.
    /// </summary>
    internal event PlayerKeepAliveCallback? OnPlayerOutgoingKeepAlive;

    /// <summary>
    /// This function is called by the hostbot when player sends a chat command.
    /// </summary>
    /// <param name="hostbot">Hostbot instance.</param>
    /// <param name="player">Player instance.</param>
    /// <param name="args">Command arguments.</param>
    /// <returns>Returns true if command executes successfully.</returns>
    internal static bool PlayerSentCommand(Hostbot hostbot, Player player, string[] args)
    {
        if (args?.Length == 0)
        {
            return false;
        }

        foreach (var (key, command) in Commands)
        {
            if (key is null || command is null)
            {
                continue;
            }

            if (key.Contains(args[0].ToLower()))
            {
                if (player.Role < command.RequiredRole)
                {
                    player.SendChat("You don't have enough permission to use this command");
                    return false;
                }

                return command.Execute(hostbot, player, args);
            }
        }

        return false;
    }

    /// <summary>
    /// This function is called when a new leave request message received.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="reason">Leave reason.</param>
    internal void LeaveRequestReceived(Player player, PlayerLeaveReason reason)
    {
        this.OnLeaveRequest?.Invoke(player, reason);
    }

    /// <summary>
    /// This function is called when a new join request message received.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="request">Join request data.</param>
    internal void JoinRequestReceived(Player player, JoinRequest request)
    {
        this.OnJoinRequest?.Invoke(player, request);
    }

    /// <summary>
    /// This function is called when map size message received.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="mapSize">Map size data.</param>
    internal void MapSizeReceived(Player player, MapSize mapSize)
    {
        this.OnMapSize?.Invoke(player, mapSize);
    }

    /// <summary>
    /// This function is called when chat to host message received.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="chatToHost">Message data.</param>
    internal void ChatToHostReceived(Player player, ChatToHost chatToHost)
    {
        this.OnChatToHost?.Invoke(player, chatToHost);
    }

    /// <summary>
    /// This function is called a player is deleted from the game.
    /// </summary>
    /// <param name="player">Player instance.</param>
    internal void PlayerDeleted(Player player)
    {
        this.OnPlayerDeleted?.Invoke(player);
    }

    /// <summary>
    /// This function is called a player requested to change it's team.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="team">Player requested team.</param>
    internal void PlayerChangeTeam(Player player, byte team)
    {
        this.OnPlayerChangeTeam?.Invoke(player, team);
    }

    /// <summary>
    /// This function is called a player requested to change it's color.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="color">Player requested color.</param>
    internal void PlayerChangeColor(Player player, byte color)
    {
        this.OnPlayerChangeColor?.Invoke(player, color);
    }

    /// <summary>
    /// This function is called a player requested to change it's race.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="race">Player requested race.</param>
    internal void PlayerChangeRace(Player player, byte race)
    {
        this.OnPlayerChangeRace?.Invoke(player, race);
    }

    /// <summary>
    /// This function is called a player requested to change it's handicap.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="handicap">Player requested handicap.</param>
    internal void PlayerChangeHandicap(Player player, byte handicap)
    {
        this.OnPlayerChangeHandicap?.Invoke(player, handicap);
    }

    /// <summary>
    /// This function is called when a player finished loading game.
    /// </summary>
    /// <param name="player">Player instance.</param>
    internal void PlayerLoaded(Player player)
    {
        this.OnPlayerLoaded?.Invoke(player);
    }

    /// <summary>
    /// This function is called when a player sends an outgoing action.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="outgoingAction">Player outgoing action.</param>
    internal void OutgoingActionReceived(Player player, OutgoingAction outgoingAction)
    {
        this.OnPlayerOutgoingAction?.Invoke(player, outgoingAction);
    }

    /// <summary>
    /// This function is called when a player sends an outgoing keep alive.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="outgoingKeepAlive">Player outgoing keep alive.</param>
    internal void OutgoingKeepAliveReceived(Player player, OutgoingKeepAlive outgoingKeepAlive)
    {
        this.OnPlayerOutgoingKeepAlive?.Invoke(player, outgoingKeepAlive);
    }
}

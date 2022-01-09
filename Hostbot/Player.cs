// <copyright file="Player.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot;

/// <summary>
/// Base player class.
/// </summary>
public class Player : IPlayer
{
    private const int ReceiveBufferSize = 2048;

    private readonly Queue<CommandMessage> incomingQueue = new();
    private readonly Queue<CommandMessage> outgoingQueue = new();
    private readonly Queue<uint> pings = new();
    private readonly GameProtocol? protocol;
    private readonly Hostbot? hostbot;
    private readonly byte[]? receiveBuffer;
    private int bufferIndex;
    private int messageRead;
    private int messageSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="Player"/> class.
    /// </summary>
    /// <param name="client">Player's tcp client.</param>
    /// <param name="protocol">Game protocol.</param>
    /// <param name="hostbot">Hostbot instance.</param>
    internal Player(TcpClient client, GameProtocol protocol, Hostbot hostbot)
    {
        var endpoint = client.Client.RemoteEndPoint as IPEndPoint;

        this.Client = client;
        this.NetworkStream = client.GetStream();
#pragma warning disable CS0618 // Type or member is obsolete

        // It is obsolete because address value is family dependent and throws exception on ipv6
        // However since we are forced to use ipv4 for warcraft protocol it is safe to use.
        this.ExternalIp = (uint)endpoint.Address.Address;
#pragma warning restore CS0618 // Type or member is obsolete
        this.Port = (ushort)endpoint.Port;
        this.protocol = protocol;
        this.hostbot = hostbot;
        this.receiveBuffer = new byte[ReceiveBufferSize];
        this.Name = "Undefined";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Player"/> class.
    /// </summary>
    /// <param name="id">Fake player id.</param>
    /// <param name="name">Fake player name.</param>
    /// <param name="slot">Fake player slot.</param>
    internal Player(byte id, string name, IGameSlot slot)
    {
        this.Id = id;
        this.Name = name;
        this.Slot = slot;
    }

    /// <inheritdoc/>
    public byte Id { get; set; }

    /// <inheritdoc/>
    public string Name { get; set; }

    /// <inheritdoc/>
    public IGameSlot? Slot { get; set; }

    /// <inheritdoc/>
    public long FinishedLoadingTicks { get; set; }

    /// <inheritdoc/>
    public bool IsLoadingFinished => this.FinishedLoadingTicks != default;

    /// <inheritdoc/>
    public bool IsDummy => this.Client is null;

    /// <inheritdoc/>
    public bool IsConnected => this.Id != default;

    /// <summary>
    /// Gets a value indicating whether this player should be deleted on the next server tick or not.
    /// </summary>
    public bool ShouldDelete { get; private set; }

    /// <summary>
    /// Gets or sets player role.
    /// </summary>
    internal Role Role { get; set; }

    /// <summary>
    /// Gets or sets internal ip.
    /// </summary>
    internal uint InternalIp { get; set; }

    /// <summary>
    /// Gets external ip.
    /// </summary>
    internal uint ExternalIp { get; }

    /// <summary>
    /// Gets player port.
    /// </summary>
    internal ushort Port { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this player download is started or not.
    /// </summary>
    internal bool IsDownloadStarted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this player download is finished or not.
    /// </summary>
    internal bool IsDownloadFinished { get; set; }

    /// <summary>
    /// Gets or sets ticks of when this player started downloading.
    /// </summary>
    internal long StartedDownloadTicks { get; set; }

    /// <summary>
    /// Gets or sets the time when this player finished downloading map.
    /// </summary>
    internal long FinishedDownloadTime { get; set; }

    /// <summary>
    /// Gets or sets the last mappart acknowledged by the player.
    /// </summary>
    internal uint LastMapPartAcked { get; set; }

    /// <summary>
    /// Gets or sets the last mappart sent to the player (for sending more than one part at a time).
    /// </summary>
    internal uint LastMapPartSent { get; set; }

    /// <summary>
    /// Gets or sets player leave reason code.
    /// </summary>
    internal PlayerLeaveReason LeaveReasonCode { get; set; }

    /// <summary>
    /// Gets number of keep alives received from this player.
    /// </summary>
    internal uint SyncCounter { get; private set; }

    /// <summary>
    /// Gets player's tcp client.
    /// </summary>
    internal TcpClient? Client { get; }

    /// <summary>
    /// Gets player unprocessed checksums.
    /// </summary>
    internal Queue<uint> Checksums { get; } = new();

    /// <summary>
    /// Gets player leave reason message.
    /// </summary>
    internal string? LeaveReasonMessage { get; private set; }

    /// <summary>
    /// Gets player's tcp network stream.
    /// Used for sending and receiving.
    /// </summary>
    protected NetworkStream? NetworkStream { get; }

    /// <inheritdoc/>
    public uint GetPing()
    {
        if (this.pings.Count == 0)
        {
            return 0;
        }

        uint average = 0;

        foreach (var ping in this.pings)
        {
            average += ping;
        }

        average /= (uint)this.pings.Count;

        return average;
    }

    /// <inheritdoc/>
    public void SendChat(string message)
    {
        this.SendChat(this.hostbot.GetHostPlayerId(), message);
    }

    /// <inheritdoc/>
    public void Delete(PlayerLeaveReason code, string reason)
    {
        this.LeaveReasonCode = code;
        this.LeaveReasonMessage = reason;
        this.ShouldDelete = true;
    }

    /// <summary>
    /// This function is called in the main game loop.
    /// No blocking is done or ever should be done here.
    /// </summary>
    internal void Update()
    {
        if (this.ShouldDelete || this.IsDummy)
        {
            return;
        }

        this.ExtractMessages();
        this.ProcessMessages();
    }

    /// <summary>
    /// Enqueues a message to send.
    /// </summary>
    /// <param name="message">Command message to send.</param>
    internal void SendMessage(CommandMessage message)
    {
        if (this.ShouldDelete || this.IsDummy || !this.IsConnected)
        {
            return;
        }

        this.outgoingQueue.Enqueue(message);
    }

    /// <summary>
    /// Sends every queued message to the player.
    /// </summary>
    internal void SendMessages()
    {
        if (this.ShouldDelete || this.IsDummy)
        {
            return;
        }

        while (this.outgoingQueue.TryDequeue(out var message))
        {
            try
            {
                this.NetworkStream.Write(message.FinalByteArray());
            }
            catch (Exception)
            {
                this.Delete(this.hostbot.GameState == GameState.Lobby ? PlayerLeaveReason.Lobby : PlayerLeaveReason.Lost, "connection lost");
            }
        }
    }

    /// <summary>
    /// Rejects a user from joining our lobby.
    /// </summary>
    /// <param name="reason">Rejection reason.</param>
    internal void RejectJoin(RejectJoinReason reason)
    {
        if (this.IsConnected || this.ShouldDelete || this.IsDummy)
        {
            return;
        }

        this.outgoingQueue.Enqueue(new RejectJoin(reason));
    }

    /// <summary>
    /// Send a private chat message to this player.
    /// </summary>
    /// <param name="fromPlayerId">From player id.</param>
    /// <param name="message">Chat message.</param>
    internal void SendChat(byte fromPlayerId, string message)
    {
        if (!this.IsConnected || this.ShouldDelete || this.IsDummy)
        {
            return;
        }

        if (this.hostbot.GameState == GameState.Lobby)
        {
            foreach (var part in message.Chunk(byte.MaxValue - 1))
            {
                this.SendMessage(new ChatFromHost(fromPlayerId, new byte[] { this.Id }, ChatToHostCommand.Message, Array.Empty<byte>(), new(part)));
            }
        }
        else
        {
            foreach (var part in message.Chunk((byte.MaxValue - 1) / 2))
            {
                var extraFlags = new byte[] { 3, 0, 0, 0 };

                if (this.Slot is not null)
                {
                    extraFlags[0] = (byte)(3 + this.Slot.Color);
                }

                this.SendMessage(new ChatFromHost(fromPlayerId, new byte[] { this.Id }, ChatToHostCommand.MessageExtra, extraFlags, new(part)));
            }
        }
    }

    /// <summary>
    /// Closes player connection.
    /// </summary>
    internal void Close()
    {
        this.Client?.Close();
    }

    private void ExtractMessages()
    {
        try
        {
            // Is there any incoming data available to read ?
            while (this.NetworkStream.DataAvailable)
            {
                if (this.bufferIndex == 0)
                {
                    // Read message header (4 bytes)
                    this.messageRead = this.NetworkStream.Read(this.receiveBuffer.AsSpan(this.bufferIndex, 4));

                    if (this.messageRead < 4 || this.receiveBuffer[0] != GameProtocol.W3GSHeaderConstant)
                    {
                        this.ShouldDelete = true;
                        return;
                    }

                    this.messageSize = (ushort)(this.receiveBuffer[3] << 8 | this.receiveBuffer[2]);
                    this.bufferIndex = 4;

                    // Check if there is still more data to read.
                    if (!this.NetworkStream.DataAvailable)
                    {
                        return;
                    }

                    this.messageRead += this.NetworkStream.Read(this.receiveBuffer.AsSpan(this.bufferIndex, this.messageSize - this.messageRead));
                }
                else
                {
                    this.messageRead += this.NetworkStream.Read(this.receiveBuffer.AsSpan(this.bufferIndex, this.messageSize - this.messageRead));
                }

                if (this.messageRead == this.messageSize)
                {
                    // Full message is received.
                    this.InsertMessage();
                }
            }
        }
        catch (Exception)
        {
            this.Delete(this.hostbot.GameState == GameState.Lobby ? PlayerLeaveReason.Lobby : PlayerLeaveReason.Lost, "connection lost");
        }
    }

    private void ProcessMessages()
    {
        while (this.incomingQueue.TryDequeue(out var message))
        {
            switch (message.Id)
            {
                case GameProtocol.W3GSJoinRequest:
                    if (this.IsConnected)
                    {
                        continue;
                    }

                    if (message is JoinRequest joinReq)
                    {
                        this.protocol.JoinRequestReceived(this, joinReq);
                    }

                    break;
                case GameProtocol.W3GSLeaveRequest:
                    if (!this.IsConnected)
                    {
                        continue;
                    }

                    if (message is LeaveRequest leaveReq)
                    {
                        this.protocol.LeaveRequestReceived(this, leaveReq.Reason);
                    }

                    break;
                case GameProtocol.W3GSGameLoadedSelf:
                    if (!this.IsConnected)
                    {
                        continue;
                    }

                    this.FinishedLoadingTicks = Utility.GetTicks();
                    this.protocol.PlayerLoaded(this);

                    break;
                case GameProtocol.W3GSOutgoingAction:
                    if (!this.IsConnected)
                    {
                        continue;
                    }

                    if (message is OutgoingAction outgoingAction)
                    {
                        this.protocol.OutgoingActionReceived(this, outgoingAction);
                    }

                    break;
                case GameProtocol.W3GSOutgoingKeepAlive:
                    if (!this.IsConnected)
                    {
                        continue;
                    }

                    if (message is OutgoingKeepAlive outgoingKeepAlive)
                    {
                        this.SyncCounter++;
                        this.Checksums.Enqueue(outgoingKeepAlive.Checksum);
                        this.protocol.OutgoingKeepAliveReceived(this, outgoingKeepAlive);
                    }

                    break;
                case GameProtocol.W3GSMapSize:
                    if (!this.IsConnected)
                    {
                        continue;
                    }

                    if (message is MapSize mapSize)
                    {
                        this.protocol.MapSizeReceived(this, mapSize);
                    }

                    break;
                case GameProtocol.W3GSChatToHost:
                    if (!this.IsConnected)
                    {
                        continue;
                    }

                    if (message is ChatToHost chatToHost)
                    {
                        this.protocol.ChatToHostReceived(this, chatToHost);
                    }

                    break;
                case GameProtocol.W3GSPongToHost:
                    if (!this.IsConnected)
                    {
                        continue;
                    }

                    if (message is PongToHost pongToHost)
                    {
                        var pong = pongToHost.Ticks;

                        if (pong == 1)
                        {
                            break;
                        }

                        if (!this.IsDownloadStarted || (this.IsDownloadFinished && Utility.GetTime() - this.FinishedDownloadTime >= 5))
                        {
                            if (this.hostbot.Players.Cast<Player>().Any(p => p.IsDownloadStarted && !p.IsDownloadFinished))
                            {
                                break;
                            }

                            this.pings.Enqueue((uint)Utility.GetTicks() - pong);

                            if (this.pings.Count > 10)
                            {
                                this.pings.Dequeue();
                            }
                        }

                        break;
                    }

                    break;
                default:
                    break;
            }
        }
    }

    private bool InsertMessage()
    {
        this.bufferIndex = 0;
        this.messageRead = 0;

        switch (this.receiveBuffer[1])
        {
            case GameProtocol.W3GSJoinRequest:
                this.incomingQueue.Enqueue(new JoinRequest(this.receiveBuffer));
                return true;
            case GameProtocol.W3GSLeaveRequest:
                this.incomingQueue.Enqueue(new LeaveRequest(this.receiveBuffer));
                return true;
            case GameProtocol.W3GSGameLoadedSelf:
                this.incomingQueue.Enqueue(new GameLoadedSelf(this.receiveBuffer));
                return true;
            case GameProtocol.W3GSOutgoingAction:
                this.incomingQueue.Enqueue(new OutgoingAction(this.receiveBuffer, this.Id));
                return true;
            case GameProtocol.W3GSOutgoingKeepAlive:
                this.incomingQueue.Enqueue(new OutgoingKeepAlive(this.receiveBuffer));
                return true;
            case GameProtocol.W3GSChatToHost:
                this.incomingQueue.Enqueue(new ChatToHost(this.receiveBuffer));
                return true;
            case GameProtocol.W3GSMapSize:
                this.incomingQueue.Enqueue(new MapSize(this.receiveBuffer));
                return true;
            case GameProtocol.W3GSPongToHost:
                this.incomingQueue.Enqueue(new PongToHost(this.receiveBuffer));
                return true;
            default:
                Log.Warning($"Unknown message received 0x{this.receiveBuffer[1]:X2}");
                return false;
        }
    }
}

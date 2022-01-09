// <copyright file="GameSlot.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft;

/// <summary>
/// Game slot used in lobby.
/// </summary>
public class GameSlot : IGameSlot
{
    /// <summary>
    /// Maximum slots allowed in a lobby.
    /// </summary>
    internal const byte MaxSlots = 24;

    private readonly Hostbot hostbot;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameSlot"/> class.
    /// </summary>
    /// <param name="hostbot">Hostbot instance.</param>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="player">Player instance.</param>
    /// <param name="downloadStatus">Download status (0% to 100%).</param>
    /// <param name="slotStatus">Slot status (0 = open, 1 = closed, 2 = occupied).</param>
    /// <param name="computer">Computer value (0 = no, 1 = yes).</param>
    /// <param name="team">Team.</param>
    /// <param name="color">Color.</param>
    /// <param name="race">Race (1 = human, 2 = orc, 4 = night elf, 8 = undead, 32 = random, 64 = selectable).</param>
    /// <param name="computerType">Computer type (0 = easy, 1 = human or normal comp, 2 = hard comp).</param>
    /// <param name="handicap">Handicap.</param>
    internal GameSlot(Hostbot hostbot, byte slotIndex, Player? player, byte downloadStatus, SlotStatus slotStatus, byte computer, byte team, byte color, SlotRace race, SlotComputerType computerType = SlotComputerType.Normal, byte handicap = 100)
    {
        this.hostbot = hostbot;
        this.SlotIndex = slotIndex;
        this.Player = player;
        this.DownloadStatus = downloadStatus;
        this.SlotStatus = slotStatus;
        this.Computer = computer;
        this.Team = team;
        this.InternalColor = color;
        this.Race = race;
        this.ComputerType = computerType;
        this.Handicap = handicap;
    }

    /// <inheritdoc/>
    public byte SlotIndex { get; }

    /// <inheritdoc/>
    public byte PlayerId
    {
        get
        {
            if (this.Player is null)
            {
                return 0;
            }

            return this.Player.Id;
        }
    }

    /// <inheritdoc/>
    public IPlayer? Player { get; set; }

    /// <inheritdoc/>
    public byte DownloadStatus { get; set; }

    /// <inheritdoc/>
    public SlotStatus SlotStatus { get; set; }

    /// <inheritdoc/>
    public byte Computer { get; set; }

    /// <inheritdoc/>
    public byte Team { get; set; }

    /// <inheritdoc/>
    public byte Color
    {
        get
        {
            return this.InternalColor;
        }

        set
        {
            if (value >= MaxSlots || value == this.InternalColor)
            {
                return;
            }

            // Make sure the requested color isn't already taken by another player.
            if (this.hostbot.Players.Any(p => p.Slot is not null && p.Slot.Color == value))
            {
                return;
            }

            var slot = (GameSlot?)this.hostbot.Slots.Find(s => s.Color == value);
            if (slot is null)
            {
                this.InternalColor = value;
            }
            else
            {
                (this.InternalColor, slot.InternalColor) = (slot.InternalColor, this.InternalColor);
            }
        }
    }

    /// <inheritdoc/>
    public SlotRace Race { get; set; }

    /// <inheritdoc/>
    public SlotComputerType ComputerType { get; set; }

    /// <inheritdoc/>
    public byte Handicap { get; set; }

    /// <summary>
    /// Gets or sets this slot's internal color value.
    /// </summary>
    /// <value>Slot's internal color value.</value>
    internal byte InternalColor { get; set; }

    /// <inheritdoc/>
    public bool Open()
    {
        if (this.hostbot.GameState != GameState.Lobby)
        {
            return false;
        }

        this.Player?.Delete(PlayerLeaveReason.Lobby, "kicked when opening a slot");
        this.Player = null;
        this.DownloadStatus = byte.MaxValue;
        this.SlotStatus = SlotStatus.Open;
        this.Computer = 0;

        this.hostbot.SendSlotsInfo();

        return true;
    }

    /// <inheritdoc/>
    public bool Close()
    {
        if (this.hostbot.GameState != GameState.Lobby)
        {
            return false;
        }

        this.Player?.Delete(PlayerLeaveReason.Lobby, "kicked when opening a slot");
        this.Player = null;
        this.DownloadStatus = byte.MaxValue;
        this.SlotStatus = SlotStatus.Closed;
        this.Computer = 0;

        this.hostbot.SendSlotsInfo();

        return true;
    }

    /// <inheritdoc/>
    public bool Swap(IGameSlot other)
    {
        if (this.hostbot.GameState != GameState.Lobby)
        {
            return false;
        }

        if (this.Player is not null)
        {
            this.Player.Slot = other;
        }

        if (other.Player is not null)
        {
            other.Player.Slot = this;
        }

        (this.Player, other.Player) = (other.Player, this.Player);
        (this.DownloadStatus, other.DownloadStatus) = (other.DownloadStatus, this.DownloadStatus);
        (this.SlotStatus, other.SlotStatus) = (other.SlotStatus, this.SlotStatus);
        (this.Computer, other.Computer) = (other.Computer, this.Computer);
        (this.ComputerType, other.ComputerType) = (other.ComputerType, this.ComputerType);

        if ((this.hostbot.Map.MapOptions & MapOptions.FixedPlayerSettings) == 0)
        {
            if ((this.hostbot.Map.MapOptions & MapOptions.CustomForces) == 0)
            {
                (this.Team, other.Team) = (other.Team, this.Team);
            }

            (this.InternalColor, ((GameSlot)other).InternalColor) = (((GameSlot)other).InternalColor, this.InternalColor);
            (this.Race, other.Race) = (other.Race, this.Race);
            (this.Handicap, other.Handicap) = (other.Handicap, this.Handicap);
        }

        this.hostbot.SendSlotsInfo();

        return true;
    }

    /// <inheritdoc/>
    public byte[] ToByteArray()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(this.PlayerId);
            writer.Write(this.DownloadStatus);
            writer.Write((byte)this.SlotStatus);
            writer.Write(this.Computer);
            writer.Write(this.Team);
            writer.Write(this.Color);
            writer.Write((byte)this.Race);
            writer.Write((byte)this.ComputerType);
            writer.Write(this.Handicap);
        }

        return stream.ToArray();
    }
}

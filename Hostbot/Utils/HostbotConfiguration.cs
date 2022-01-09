// <copyright file="HostbotConfiguration.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Utils;

/// <summary>
/// Configuration data for a game.
/// </summary>
[Serializable]
internal class HostbotConfiguration
{
    /// <summary>
    /// Gets or sets bot game name.
    /// </summary>
    public string GameName { get; set; } = "|cFF6495EDIcecrown Melee";

    /// <summary>
    /// Gets or sets map name.
    /// </summary>
    public string Map { get; set; } = "Maps\\(12)IceCrown.w3m";

    /// <summary>
    /// Gets or sets map HCL (Ex: game mode in DotA).
    /// </summary>
    public string HCL { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets max lobbies for this game.
    /// </summary>
    public byte MaxLobbies { get; set; } = 1;
}

// <copyright file="PlayerLeaveReason.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Shared.Warcraft;

/// <summary>
/// Warcraft player leave reason.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "Reviewed")]
public enum PlayerLeaveReason : uint
{
    Disconnect = 1,
    Lost = 7,
    LostBuildings = 8,
    Won = 9,
    Draw = 10,
    Observer = 11,
    Lobby = 13,
    GProxy = 100,
}

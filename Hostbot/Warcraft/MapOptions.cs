// <copyright file="MapOptions.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft;

/// <summary>
/// Warcraft map options.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "Reviewed")]
[Flags]
internal enum MapOptions : uint
{
    None = 0,
    HideMinimap = 1 << 0,
    ModifyAllyPriorities = 1 << 1,
    Melee = 1 << 2,
    RevealTerrain = 1 << 4,
    FixedPlayerSettings = 1 << 5,
    CustomForces = 1 << 6,
    CustomTechTree = 1 << 7,
    CustomAbilities = 1 << 8,
    CustomUpgrades = 1 << 9,
    WaterWavesOnCliffShores = 1 << 11,
    WaterWavesOnSlopeShores = 1 << 12,
}

// <copyright file="MapGameType.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft;

/// <summary>
/// Warcraft map game type.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "Reviewed")]
[Flags]
internal enum MapGameType : uint
{
    None = 0,
    Unknown = 1,
    Blizzard = 1 << 3,
    Melee = 1 << 5,
    SavedGame = 1 << 9,
    PrivateGame = 1 << 11,
    MakerUser = 1 << 13,
    MakerBlizzard = 1 << 14,
    TypeMelee = 1 << 15,
    TypeScenario = 1 << 16,
    SizeSmall = 1 << 17,
    SizeMedium = 1 << 18,
    SizeLarge = 1 << 19,
    ObsFull = 1 << 20,
    ObsOnDeath = 1 << 21,
    ObsNone = 1 << 22,
}

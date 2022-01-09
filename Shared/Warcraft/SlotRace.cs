// <copyright file="SlotRace.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Shared.Warcraft;

/// <summary>
/// Warcraft slot race.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "Reviewed")]
[Flags]
public enum SlotRace : byte
{
    None = 0,
    Human = 1,
    Orc = 2,
    Nightelf = 4,
    Undead = 8,
    Random = 32,
    Selectable = 64,
}

// <copyright file="MapFlags.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft;

/// <summary>
/// Warcraft map flags.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "Reviewed")]
[Flags]
internal enum MapFlags : byte
{
    None = 0,
    TeamsTogether = 1,
    FixedTeams = 2,
    ShareUnits = 4,
    RandomHero = 8,
    RandomRaces = 16,
}

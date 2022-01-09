// <copyright file="MapVisibility.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft;

/// <summary>
/// Warcraft map visibility flags.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "Reviewed")]
internal enum MapVisibility
{
    HideTerrain = 1,
    Explored = 2,
    AlwaysVisible = 3,
    Default = 4,
}

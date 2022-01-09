// <copyright file="SlotStatus.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Shared.Warcraft;

/// <summary>
/// Warcraft game slot status.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "Reviewed")]
public enum SlotStatus : byte
{
    Open,
    Closed,
    Occupied,
}

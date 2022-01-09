// <copyright file="MapFilterType.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft
{
    /// <summary>
    /// Warcraft map filter type.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "Reviewed")]
    [Flags]
    internal enum MapFilterType : byte
    {
        None = 0,
        Melee = 1,
        Scenario = 2,
    }
}

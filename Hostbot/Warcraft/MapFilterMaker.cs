// <copyright file="MapFilterMaker.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft
{
    /// <summary>
    /// Warcraft map filter maker.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "Reviewed")]
    [Flags]
    internal enum MapFilterMaker
    {
        None = 0,
        User = 1,
        Blizzard = 2,
    }
}

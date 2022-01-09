// <copyright file="MapFilterSize.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft
{
    /// <summary>
    /// Warcraft map filter maker.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "Reviewed")]
    [Flags]
    internal enum MapFilterSize
    {
        None = 0,
        Small = 1,
        Medium = 2,
        Large = 4,
    }
}

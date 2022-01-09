// <copyright file="MapFilterObservers.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft
{
    /// <summary>
    /// Warcraft map filter observers.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "Reviewed")]
    [Flags]
    internal enum MapFilterObservers
    {
        Meme = 0,
        Full = 1,
        OnDeath = 2,
        None = 4,
    }
}

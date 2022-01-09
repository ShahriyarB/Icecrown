// <copyright file="ChatToHostCommand.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft
{
    /// <summary>
    /// Warcraft chat to host commands.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "Reviewed")]
    internal enum ChatToHostCommand : byte
    {
        Message = 16,
        ChangeTeam = 17,
        ChangeColor = 18,
        ChangeRace = 19,
        ChangeHandicap = 20,
        MessageExtra = 32,
    }
}

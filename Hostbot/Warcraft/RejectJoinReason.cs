// <copyright file="RejectJoinReason.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft;

/// <summary>
/// Warcraft lobby reject join result.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "Reviewed")]
internal enum RejectJoinReason : uint
{
    Invalid = 0x7,
    Full = 0x9,
    Started = 0x10,
    WrongPassword = 0x27,
}

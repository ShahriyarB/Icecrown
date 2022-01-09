// <copyright file="ICommand.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Shared;

/// <summary>
/// Chat command interface.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Gets required role to execute this command.
    /// </summary>
    Role RequiredRole { get; }

    /// <summary>
    /// Gets command description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets command correct usage.
    /// </summary>
    string Usage { get; }

    /// <summary>
    /// Execute this command.
    /// </summary>
    /// <param name="hostbot">Hostbot instance.</param>
    /// <param name="player">Player instance.</param>
    /// <param name="args">Command arguments.</param>
    /// <returns>Returns true if command executes successfully.</returns>
    public bool Execute(IHostbot hostbot, IPlayer player, string[]? args = null);
}

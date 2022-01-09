// <copyright file="AbortCommand.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Commands
{
    /// <summary>
    /// "abort" command class.
    /// </summary>
    internal class AbortCommand : ICommand
    {
        /// <inheritdoc/>
        public Role RequiredRole => Role.Moderator;

        /// <inheritdoc/>
        public string Description => "This command tries to stop the start counter";

        /// <inheritdoc/>
        public string Usage => "abort";

        /// <inheritdoc/>
        public bool Execute(IHostbot hostbot, IPlayer player, string[]? args = null)
        {
            hostbot.StopCountdown();
            return true;
        }
    }
}

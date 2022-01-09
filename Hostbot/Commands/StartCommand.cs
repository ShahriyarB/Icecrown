// <copyright file="StartCommand.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Commands
{
    /// <summary>
    /// "start" command class.
    /// </summary>
    internal class StartCommand : ICommand
    {
        /// <inheritdoc/>
        public Role RequiredRole => Role.Moderator;

        /// <inheritdoc/>
        public string Description => "This command tries to start the game";

        /// <inheritdoc/>
        public string Usage => "start";

        /// <inheritdoc/>
        public bool Execute(IHostbot hostbot, IPlayer player, string[]? args = null)
        {
            return hostbot.StartCountdown();
        }
    }
}

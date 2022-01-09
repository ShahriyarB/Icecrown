// <copyright file="PingCommand.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Commands
{
    /// <summary>
    /// "ping" command class.
    /// </summary>
    internal class PingCommand : ICommand
    {
        /// <inheritdoc/>
        public Role RequiredRole => Role.User;

        /// <inheritdoc/>
        public string Description => "This command tells your current ping to server";

        /// <inheritdoc/>
        public string Usage => "ping";

        /// <inheritdoc/>
        public bool Execute(IHostbot hostbot, IPlayer player, string[]? args)
        {
            player.SendChat($"Your ping is {player.GetPing()}");
            return true;
        }
    }
}

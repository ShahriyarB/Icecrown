// <copyright file="VersionCommand.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Commands
{
    /// <summary>
    /// "version" command class.
    /// </summary>
    internal class VersionCommand : ICommand
    {
        /// <inheritdoc/>
        public Role RequiredRole => Role.User;

        /// <inheritdoc/>
        public string Description => "This command tells version of the bot";

        /// <inheritdoc/>
        public string Usage => "version";

        /// <inheritdoc/>
        public bool Execute(IHostbot hostbot, IPlayer player, string[]? args = null)
        {
            player.SendChat($"Hostbot version: {Program.Version}");
            return true;
        }
    }
}

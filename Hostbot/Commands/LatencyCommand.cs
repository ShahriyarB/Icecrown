// <copyright file="LatencyCommand.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Commands
{
    /// <summary>
    /// "latency" command class.
    /// </summary>
    internal class LatencyCommand : ICommand
    {
        /// <inheritdoc/>
        public Role RequiredRole => Role.Moderator;

        /// <inheritdoc/>
        public string Description => "This command sets game latency (tick rate).";

        /// <inheritdoc/>
        public string Usage => "latency latency_number (between 20 and 500)";

        /// <inheritdoc/>
        public bool Execute(IHostbot hostbot, IPlayer player, string[]? args)
        {
            if (args?.Length < 2)
            {
                player.SendChat($"Current latency is {hostbot.Latency}");
                return true;
            }

            if (!ushort.TryParse(args[1], out var latency))
            {
                player.SendChat("Invalid latency format");
                return false;
            }

            if (latency is < 50 or > 500)
            {
                player.SendChat("Invalid latency range. latency can't be less than 50 or more than 500");
                return false;
            }

            hostbot.Latency = latency;
            player.SendChat($"Latency updated to {latency}");
            return true;
        }
    }
}

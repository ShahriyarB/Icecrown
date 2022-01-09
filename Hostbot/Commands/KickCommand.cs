// <copyright file="KickCommand.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Commands
{
    /// <summary>
    /// "kick" command class.
    /// </summary>
    internal class KickCommand : ICommand
    {
        /// <inheritdoc/>
        public Role RequiredRole => Role.Moderator;

        /// <inheritdoc/>
        public string Description => "This command kicks an many users as specified";

        /// <inheritdoc/>
        public string Usage => "kick name another_name ...";

        /// <inheritdoc/>
        public bool Execute(IHostbot hostbot, IPlayer player, string[]? args)
        {
            if (args?.Length < 2)
            {
                player.SendChat("Invalid command usage");
                player.SendChat($"Correct format is {Settings.Current.CommandDelimiter}{this.Usage}");
                return false;
            }

            foreach (var arg in args[1..])
            {
                var matches = hostbot.Players.Where(p => p.Name.Contains(arg, StringComparison.OrdinalIgnoreCase));

                switch (matches.Count())
                {
                    case 0:
                        player.SendChat($"No player matched for \"{arg}\"");
                        continue;
                    case > 1:
                        player.SendChat($"More than one player matched for \"{arg}\"");
                        continue;
                }

                var match = matches.First();

                if (match.Id == player.Id)
                {
                    player.SendChat("You can't kick yourself");
                    continue;
                }

                match.Delete(hostbot.GameState == GameState.Lobby ? PlayerLeaveReason.Lobby : PlayerLeaveReason.Lost, $"kicked by {player.Name}");
                hostbot.SendAllChat($"{player.Name} kicked {match.Name}");
            }

            return true;
        }
    }
}

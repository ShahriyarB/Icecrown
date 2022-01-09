// <copyright file="DummyCommand.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Commands
{
    /// <summary>
    /// "dummy" command class.
    /// </summary>
    internal class DummyCommand : ICommand
    {
        /// <inheritdoc/>
        public Role RequiredRole => Role.Administrator;

        /// <inheritdoc/>
        public string Description => "This command add/remove dummies (fake players) to/from the lobby";

        /// <inheritdoc/>
        public string Usage => "dummy add | fill | remove (all)";

        /// <inheritdoc/>
        public bool Execute(IHostbot hostbot, IPlayer player, string[]? args)
        {
            if (args?.Length < 2)
            {
                player.SendChat("Invalid command usage");
                player.SendChat($"Correct format is {Settings.Current.CommandDelimiter}{this.Usage}");
                return false;
            }

            switch (args[1].ToLower())
            {
                case "add":
                    hostbot.InsertDummy(false);
                    break;
                case "fill":
                    hostbot.InsertDummy(true);
                    break;
                case "remove":
                    hostbot.RemoveDummy(args.Length > 2 && string.Equals(args[2], "all", StringComparison.OrdinalIgnoreCase));
                    break;
                default:
                    player.SendChat("Invalid command usage");
                    player.SendChat($"Correct format is {Settings.Current.CommandDelimiter}{this.Usage}");
                    return false;
            }

            return true;
        }
    }
}

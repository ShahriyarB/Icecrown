// <copyright file="HelpCommand.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Commands
{
    /// <summary>
    /// "help" command class.
    /// </summary>
    internal class HelpCommand : ICommand
    {
        /// <inheritdoc/>
        public Role RequiredRole => Role.User;

        /// <inheritdoc/>
        public string Description => "This command displays all available commands and their usage";

        /// <inheritdoc/>
        public string Usage => "help [command]";

        /// <inheritdoc/>
        public bool Execute(IHostbot hostbot, IPlayer player, string[]? args = null)
        {
            if (args?.Length > 1)
            {
                foreach (var (key, value) in GameProtocol.Commands)
                {
                    if (key.Contains(args[1].ToLower()))
                    {
                        foreach (var str in GetCommandHelp(key, value))
                        {
                            player.SendChat(str);
                        }

                        return true;
                    }
                }

                player.SendChat("Command not found");
                return false;
            }
            else
            {
                var index = 0;
                foreach (var (key, value) in GameProtocol.Commands)
                {
                    foreach (var str in GetCommandHelp(key, value))
                    {
                        player.SendChat(str);
                    }

                    if (index++ < GameProtocol.Commands.Count - 1)
                    {
                        player.SendChat("===========================================================");
                    }
                }

                return true;
            }
        }

        private static string[] GetCommandHelp(string[] keys, ICommand command)
        {
            var help = new List<string>
            {
                $"Command: {keys[0]}",
            };

            if (keys.Length > 1)
            {
                help.Add($"Aliases: {string.Join(", ", keys.Skip(1))}");
            }
            else
            {
                help.Add("Aliases: None");
            }

            help.Add($"Description: {command.Description}");
            help.Add($"Usage: {command.Usage}");
            help.Add($"Required role: {command.RequiredRole}");

            return help.ToArray();
        }
    }
}

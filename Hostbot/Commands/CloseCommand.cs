﻿// <copyright file="CloseCommand.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Commands
{
    /// <summary>
    /// "close" command class.
    /// </summary>
    internal class CloseCommand : ICommand
    {
        /// <inheritdoc/>
        public Role RequiredRole => Role.Moderator;

        /// <inheritdoc/>
        public string Description => "This command closes an many slots as specified";

        /// <inheritdoc/>
        public string Usage => "close slot_id another_slot_id ...";

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
                if (byte.TryParse(arg, out var slotId))
                {
                    if (slotId == 0)
                    {
                        continue;
                    }

                    var index = slotId - 1;

                    if (index >= hostbot.Slots.Count)
                    {
                        continue;
                    }

                    hostbot.Slots[index].Close();
                }
            }

            return true;
        }
    }
}

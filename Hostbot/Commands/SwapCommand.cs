// <copyright file="SwapCommand.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Commands
{
    /// <summary>
    /// "swap" command class.
    /// </summary>
    internal class SwapCommand : ICommand
    {
        /// <inheritdoc/>
        public Role RequiredRole => Role.Moderator;

        /// <inheritdoc/>
        public string Description => "This command swaps two slots as specified";

        /// <inheritdoc/>
        public string Usage => "swap first_slot_index second_slot_index";

        /// <inheritdoc/>
        public bool Execute(IHostbot hostbot, IPlayer player, string[]? args)
        {
            if (args?.Length != 3)
            {
                player.SendChat("Invalid command usage");
                player.SendChat($"Correct format is {Settings.Current.CommandDelimiter}{this.Usage}");
                return false;
            }

            if (byte.TryParse(args[1], out var firstSlotId) && byte.TryParse(args[2], out var secondSlotId))
            {
                var firstIndex = firstSlotId - 1;
                var secondIndex = secondSlotId - 1;

                if (firstIndex < 0 || secondIndex < 0 || firstIndex >= hostbot.Slots.Count || secondIndex >= hostbot.Slots.Count)
                {
                    player.SendChat("Invalid slot index");
                    return false;
                }

                hostbot.Slots[firstIndex].Swap(hostbot.Slots[secondIndex]);
            }
            else
            {
                player.SendChat("Invalid slot index format (it should be a number between 1 and slots count)");
                return false;
            }

            return true;
        }
    }
}

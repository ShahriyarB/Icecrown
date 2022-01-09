// <copyright file="HCLCommand.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Commands
{
    /// <summary>
    /// "hcl" command class.
    /// </summary>
    internal class HCLCommand : ICommand
    {
        /// <inheritdoc/>
        public Role RequiredRole => Role.Moderator;

        /// <inheritdoc/>
        public string Description => "This command changes map HCL";

        /// <inheritdoc/>
        public string Usage => "hcl hcl_str OR hcl clear";

        /// <inheritdoc/>
        public bool Execute(IHostbot hostbot, IPlayer player, string[]? args)
        {
            if (args?.Length < 2)
            {
                if (string.IsNullOrEmpty(hostbot.MapHCL))
                {
                    player.SendChat("No HCL is set");
                }
                else
                {
                    player.SendChat($"Map HCL: {hostbot.MapHCL}");
                }

                return true;
            }

            // This prevents the word "clear" from being used as an HCL.
            // But we no longer have to register another command just to clear the HCL.
            if (string.Equals(args[1], "clear", StringComparison.OrdinalIgnoreCase))
            {
                hostbot.MapHCL = string.Empty;
                player.SendChat("Map HCL cleared");
                return true;
            }

            const string hclChars = "abcdefghijklmnopqrstuvwxyz0123456789 -=,.";

            if (args[1].Any(c => !hclChars.Contains(c)))
            {
                player.SendChat("Invalid characters in HCL");
                player.SendChat($"Valid characters for HCL are {hclChars}");
                return false;
            }

            if (args[1].Length > hostbot.Slots.Count)
            {
                player.SendChat("HCL string is too long");
                return false;
            }

            hostbot.MapHCL = args[1];
            player.SendChat($"Map HCL set to {hclChars}");

            return true;
        }
    }
}

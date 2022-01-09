// <copyright file="GameLoadedSelf.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS game loaded self message.
/// </summary>
internal class GameLoadedSelf : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameLoadedSelf"/> class.
    /// </summary>
    /// <param name="memory">Memory region containing message data.</param>
    public GameLoadedSelf(Memory<byte> memory)
        : base(memory)
    {
    }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        return Array.Empty<byte>();
    }
}

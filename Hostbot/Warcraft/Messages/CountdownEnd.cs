// <copyright file="CountdownEnd.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS countdown end message.
/// </summary>
internal class CountdownEnd : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CountdownEnd"/> class.
    /// </summary>
    public CountdownEnd()
    {
        this.Id = GameProtocol.W3GSCountdownEnd;
    }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        return Array.Empty<byte>();
    }
}

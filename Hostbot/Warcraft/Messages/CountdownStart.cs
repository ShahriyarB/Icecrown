// <copyright file="CountdownStart.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Warcraft.Messages;

/// <summary>
/// W3GS countdown start message.
/// </summary>
internal class CountdownStart : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CountdownStart"/> class.
    /// </summary>
    public CountdownStart()
    {
        this.Id = GameProtocol.W3GSCountdownStart;
    }

    /// <inheritdoc/>
    internal override byte[] ToByteArray()
    {
        return Array.Empty<byte>();
    }
}

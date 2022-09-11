﻿using Discord.Commands;
using Discord.WebSocket;
using System;

namespace FetaWarrior.DiscordFunctionality;

[Obsolete("This is probably never making it anyway")]
public class TimestampedSocketCommandContext : SocketCommandContext
{
    public DateTime CommandReceivedTime { get; }

    public TimestampedSocketCommandContext(DiscordSocketClient client, SocketUserMessage message, DateTime receivedTime)
        : base(client, message)
    {
        CommandReceivedTime = receivedTime;
    }

    public TimeSpan GetRetrievalLatency(TimeSpan discordClockOffset) => CommandReceivedTime.ToUniversalTime() - Message.Timestamp + discordClockOffset;
}

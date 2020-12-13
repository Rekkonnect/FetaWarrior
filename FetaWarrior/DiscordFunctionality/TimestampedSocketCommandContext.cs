using Discord.Commands;
using Discord.WebSocket;
using System;

namespace FetaWarrior.DiscordFunctionality
{
    public class TimestampedSocketCommandContext : SocketCommandContext
    {
        public DateTime CommandReceivedTime { get; }
        public TimeSpan RetrievalLatency => CommandReceivedTime.ToUniversalTime() - Message.Timestamp;

        public TimestampedSocketCommandContext(DiscordSocketClient client, SocketUserMessage message, DateTime receivedTime)
            : base(client, message)
        {
            CommandReceivedTime = receivedTime;
        }
    }
}

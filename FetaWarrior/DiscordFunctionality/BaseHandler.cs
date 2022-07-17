using Discord;
using Discord.WebSocket;
using FetaWarrior.Extensions;
using System;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

#nullable enable

public abstract class BaseHandler
{
    protected abstract string HandledObjectName { get; }

    protected virtual Func<SocketMessage, Task>? MessageReceivedEvents => null;

    public void AddEvents(DiscordSocketClient client)
    {
        client.MessageReceived += MessageReceivedEvents;

        AddAdditionalEvents(client);
    }

    protected virtual void AddAdditionalEvents(DiscordSocketClient client) { }

    protected void LogHandledMessage(IMessage message)
    {
        ConsoleLogging.WriteEventWithCurrentTime($"{message.Author.ToNiceString()} sent a {HandledObjectName}:\n{message.Content}");
    }
}

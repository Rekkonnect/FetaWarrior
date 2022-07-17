using Discord.Rest;
using Discord.WebSocket;

namespace FetaWarrior.DiscordFunctionality;

public abstract class InitializablePersistentMessage : PersistentMessage
{
    protected InitializablePersistentMessage(RestUserMessage currentMessage)
        : base(currentMessage) { }
    protected InitializablePersistentMessage(ISocketMessageChannel channel)
        : base()
    {
        InitializeForChannel(channel, GetInitializationMessageContent());
    }

    protected abstract string GetInitializationMessageContent();
}

using Discord;
using Discord.WebSocket;

namespace FetaWarrior.DiscordFunctionality;

public abstract class InitializablePersistentMessage : PersistentMessage
{
    protected InitializablePersistentMessage(IUserMessage currentMessage)
        : base(currentMessage) { }
    protected InitializablePersistentMessage(ISocketMessageChannel channel)
        : base()
    {
        InitializeForChannel(channel, GetInitializationMessageContent());
    }
    protected InitializablePersistentMessage(IDiscordInteraction interaction)
        : base(interaction) { }

    protected abstract string GetInitializationMessageContent();
}

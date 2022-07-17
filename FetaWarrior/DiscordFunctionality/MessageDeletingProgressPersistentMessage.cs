using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace FetaWarrior.DiscordFunctionality;

public class MessageDeletingProgressPersistentMessage : ProgressPersistentMessage
{
    public sealed override IActionLexemes Lexemes => new MessageDeletingActionLexemes();

    public MessageDeletingProgressPersistentMessage(RestUserMessage currentMessage)
        : base(currentMessage) { }
    public MessageDeletingProgressPersistentMessage(ISocketMessageChannel channel)
        : base(channel) { }

    private struct MessageDeletingActionLexemes : IActionLexemes
    {
        public string ObjectName => "message";
        public string ObjectNamePlural => "messages";

        public string ActionName => "delete";
        public string ActionPastParticiple => "deleted";
    }
}

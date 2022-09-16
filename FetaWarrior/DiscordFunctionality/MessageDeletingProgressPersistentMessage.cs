using Discord;
using Discord.WebSocket;

namespace FetaWarrior.DiscordFunctionality;

public class MessageDeletingProgressPersistentMessage : ProgressPersistentMessage
{
    public sealed override IActionLexemes Lexemes => MessageDeletingActionLexemes.Instance;

    public MessageDeletingProgressPersistentMessage(IUserMessage currentMessage)
        : base(currentMessage) { }
    public MessageDeletingProgressPersistentMessage(ISocketMessageChannel channel)
        : base(channel) { }
    public MessageDeletingProgressPersistentMessage(IDiscordInteraction interaction)
        : base(interaction) { }

    private sealed class MessageDeletingActionLexemes : IActionLexemes
    {
        public static MessageDeletingActionLexemes Instance { get; } = new();
        private MessageDeletingActionLexemes() { }

        public string ObjectName => "message";
        public string ObjectNamePlural => "messages";

        public string ActionName => "delete";
        public string ActionPastParticiple => "deleted";
    }
}

using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

public class DMHandler : BaseHandler
{
    public static DMHandler GlobalInstance { get; } = new();

    protected override string HandledObjectName => "DM";

    protected override Func<SocketMessage, Task> MessageReceivedEvents => HandleDM;

    private async Task HandleDM(SocketMessage message)
    {
        // Avoid feedbacking when the bot sends messages
        if (message.Author.Id == BotClientManager.Instance.Client.CurrentUser.Id)
            return;

        if (message.Channel is not IDMChannel)
            return;

        LogHandledMessage(message);
    }
}

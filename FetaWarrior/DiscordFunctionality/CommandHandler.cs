using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using FetaWarrior.Configuration;
using FetaWarrior.DiscordFunctionality.OldModules;
using FetaWarrior.Extensions;
using Garyon.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

[Obsolete("Old commands are ditched")]
public class CommandHandler : BaseHandler
{
    public static CommandHandler GlobalInstance { get; private set; }

    public static IEnumerable<CommandInfo> AllAvailableCommands => GlobalInstance.CommandService.Commands;
    public static IEnumerable<CommandInfo> AllPubliclyAvailableCommands => AllAvailableCommands.Where(c => !c.HasPrecondition<RequireOwnerAttribute>());

    public static DiscordSocketClient Client => BotClientManager.Instance.Client;
    public static DiscordRestClient RestClient => BotClientManager.Instance.RestClient;

    static CommandHandler()
    {
        // Nyun-nyun
        GlobalInstance = new(new());
    }

    public CommandService CommandService { get; init; }

    protected override string HandledObjectName => "command";

    protected override Func<SocketMessage, Task> MessageReceivedEvents => HandleCommandAsync;

    public CommandHandler(CommandService service)
    {
        CommandService = service;
        Initialize();
    }

    #region Initialization
    private void Initialize()
    {
        InitializeCommandHandling().Wait();
    }
    private async Task InitializeCommandHandling()
    {
        var thisAssembly = Assembly.GetEntryAssembly();
        CommandService.AddTypeReaders(thisAssembly);
        await CommandService.AddModulesAsync(thisAssembly, null);
    }
    #endregion

    #region Handlers
    private async Task HandleCommandAsync(SocketMessage message)
    {
        var receivedTime = DateTime.Now;

        var socketMessage = message as SocketUserMessage;

        if (socketMessage?.Author.IsHuman() != true)
            return;

        var context = new TimestampedSocketCommandContext(Client, socketMessage, receivedTime);

        var prefix = BotPrefixesConfig.Instance.GetPrefixForGuild(context.Guild);

        int argumentPosition = 0;
        int? customArgumentPosition = null;
        if (socketMessage.HasMentionPrefix(BotClientManager.Instance.Client.CurrentUser, ref argumentPosition))
        {
            // Skip all whitespace, not just the first
            while (socketMessage.Content[argumentPosition].IsWhiteSpace())
            {
                argumentPosition++;
            }

            customArgumentPosition = argumentPosition;
        }
        else
        {
            if (!socketMessage.Content.StartsWith(prefix))
                return;
        }
        
        LogHandledMessage(message);

        ExecuteCommandAsync(context, prefix, customArgumentPosition);
    }
    private async Task ExecuteCommandAsync(ICommandContext context, string prefix, int? customArgumentPosition)
    {
        int argumentPosition = customArgumentPosition ?? prefix.Length;
        var result = await CommandService.ExecuteAsync(context, argumentPosition, null);
        await HandleResult(context, prefix, result);
    }
    #endregion
}

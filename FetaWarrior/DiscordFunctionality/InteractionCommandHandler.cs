using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using FetaWarrior.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

public class InteractionCommandHandler : BaseHandler
{
    public static InteractionCommandHandler GlobalInstance { get; } = new();

    public static DiscordSocketClient Client => BotClientManager.Instance.Client;
    public static DiscordRestClient RestClient => BotClientManager.Instance.RestClient;
    public static InteractionService InteractionService => BotClientManager.Instance.InteractionService;

    protected override string HandledObjectName => "interaction command";

    private InteractionCommandHandler() { }

    #region Initialization
    protected override void AddAdditionalEvents(DiscordSocketClient client)
    {
        client.InteractionCreated += HandleInteraction;
    }
    #endregion

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        var interactionTask = RunInteraction(interaction); 
        LogHandledMessage(interaction);
        await interactionTask;
    }

    private static async Task RunInteraction(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(Client, interaction);
        var result = await InteractionService.ExecuteCommandAsync(context, null);
        await HandleResult(interaction, result);
    }

    private void LogHandledMessage(IDiscordInteraction interaction)
    {
        switch (interaction)
        {
            case ISlashCommandInteraction slash:
                LogHandledMessage(slash);
                break;
        }
    }

    private void LogHandledMessage(ISlashCommandInteraction interaction)
    {
        ConsoleLogging.WriteEventWithCurrentTime($"{interaction.User.ToNiceString()} sent an interaction:");
        PrintNiceInteractionMessage(interaction.Data);
    }

    private static void PrintNiceInteractionMessage(IApplicationCommandInteractionData data)
    {
        var fullCommandName = GetFullCommandName(data, out var deepmostCommand);
        Console.WriteLine($"/{fullCommandName}");
        foreach (var option in deepmostCommand?.Options ?? data.Options)
        {
            Console.WriteLine($"  {option.Name}: {option.Value} ({option.Type})");
        }
    }
    // Now move those to extensions, what an API to deal with
    // They were just this close to building a great abstracted API
    private static string GetFullCommandName(IApplicationCommandInteractionData data, out IApplicationCommandInteractionDataOption nestedSubCommand)
    {
        var commandString = data.Name;
        var options = data.Options;
        nestedSubCommand = null;
        while (true)
        {
            var next = options.FirstOrDefault();

            bool isSubcommand = IsSubCommand(next?.Type);
            if (!isSubcommand)
                break;

            commandString += $" {next.Name}";
            options = next.Options;
            nestedSubCommand = next;
        }
        return commandString;
    }

    private static bool IsSubCommand(ApplicationCommandOptionType? type)
    {
        return type is ApplicationCommandOptionType.SubCommandGroup or ApplicationCommandOptionType.SubCommand;
    }

    private static async Task HandleResult(IDiscordInteraction context, IResult result)
    {
        await HandleResult(SendMessageAsync, result);

        async Task SendMessageAsync(string message)
        {
            await context.RespondAsync(message);
        }
    }

    protected static async Task HandleResult(Func<string, Task> messageSender, IResult result)
    {
        if (result.IsSuccess)
            return;

        var error = result.Error;
        Console.WriteLine(error);

        switch (error)
        {
            case InteractionCommandError.UnknownCommand:
                break;

            case InteractionCommandError.UnmetPrecondition:
                await messageSender($"{result.ErrorReason}");
                break;

            default:
                await messageSender(error switch
                {
                    InteractionCommandError.ConvertFailed => $"Failed to convert the arguments to their correct form, make sure you provide a valid snowflake ID.",
                    InteractionCommandError.ParseFailed => $"Failed to parse the command, is this supposed to be triggered?",
                    InteractionCommandError.UnmetPrecondition => $"Failed to execute the command, either because this is not for you, or this is not the right place to do it.",

                    InteractionCommandError.Exception or
                    InteractionCommandError.Unsuccessful => $"Developer is bad, error was caused by his fault.\nError information: {error} - {result.ErrorReason}",

                    _ => "Unknown issue occurred.",
                });
                break;
        }

        if (result is PreconditionResult preconditionResult)
        {
            //preconditionResult.
        }

        if (result is ExecuteResult executionResult)
        {
            Console.WriteLine();
            Console.WriteLine(executionResult.Exception);
            Console.WriteLine();
            Console.WriteLine(executionResult.Exception.StackTrace);
            Console.WriteLine();
            Console.WriteLine(executionResult.Exception.Message);
            Console.WriteLine();
        }
    }
}
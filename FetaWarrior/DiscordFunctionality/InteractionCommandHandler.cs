using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using FetaWarrior.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

public class InteractionCommandHandler : BaseHandler
{
    public static InteractionCommandHandler GlobalInstance { get; } = new();

    public static DiscordSocketClient Client => BotClientManager.Instance.Client;
    public static DiscordRestClient RestClient => BotClientManager.Instance.RestClient;
    public static InteractionService InteractionService => BotClientManager.Instance.InteractionService;

    protected override string HandledObjectName => "interaction command";

    private readonly HashSet<IDiscordInteraction> unhandledInteractions = new();

    private InteractionCommandHandler() { }

    #region Initialization
    protected override void AddAdditionalEvents(DiscordSocketClient client)
    {
        client.InteractionCreated += HandleInteraction;
    }

    #endregion

    public void RegisterCommandExecution(SocketInteractionModule module)
    {
        unhandledInteractions.Remove(module.Context.Interaction);
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        var interactionTask = RunInteraction(interaction);
        LogHandledMessage(interaction);
        await interactionTask;
    }

    private async Task RunInteraction(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(Client, interaction);
        unhandledInteractions.Add(interaction);
        var result = await InteractionService.ExecuteCommandAsync(context, null);

        // Delay for a bit to guarantee that the command execution event fires
        await Task.Delay(2000);
        if (unhandledInteractions.Contains(interaction))
        {
            result = PreconditionResult.FromError("You cannot execute this command because either you or the bot does not have the sufficient permissions.");
            unhandledInteractions.Remove(interaction);
        }
        await HandleResult(interaction, result);
    }

    private static void LogHandledMessage(IDiscordInteraction interaction)
    {
        switch (interaction)
        {
            case ISlashCommandInteraction slash:
                LogHandledMessage(slash);
                break;
        }
    }

    private static void LogHandledMessage(ISlashCommandInteraction interaction)
    {
        ConsoleLogging.WriteEventWithCurrentTime($"{interaction.User.ToNiceString()} sent an interaction:");
        PrintNiceInteractionMessage(interaction.Data);
        Console.WriteLine();
    }

    private static void PrintNiceInteractionMessage(IApplicationCommandInteractionData data)
    {
        var fullCommandName = data.GetFullCommandName(out var deepmostCommand);
        Console.WriteLine($"/{fullCommandName}");
        foreach (var option in deepmostCommand?.Options ?? data.Options)
        {
            Console.WriteLine($"  {option.Name}: {option.Value} ({option.Type})");
        }
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
                await messageSender("Failed to execute the command, either because this is not for you, or this is not the right place to do it.");
                break;

            default:
                await messageSender(error switch
                {
                    InteractionCommandError.ConvertFailed => "Failed to convert the arguments to their correct form, make sure you provide a valid snowflake ID.",
                    InteractionCommandError.ParseFailed => "Failed to parse the command, is this supposed to be triggered?",

                    InteractionCommandError.Exception or
                    InteractionCommandError.Unsuccessful => $"Developer is bad, error was caused by his fault.\nError information: {error} - {result.ErrorReason}",

                    _ => "Unknown issue occurred.",
                });
                break;
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
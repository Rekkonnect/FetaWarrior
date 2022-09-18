using Discord;
using Discord.Commands;
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
        if (MessageReceivedEvents is not null)
            client.MessageReceived += MessageReceivedEvents;

        AddAdditionalEvents(client);
    }

    protected virtual void AddAdditionalEvents(DiscordSocketClient client) { }

    protected void LogHandledMessage(IMessage message)
    {
        ConsoleLogging.WriteEventWithCurrentTime($"{message.Author.ToNiceString()} sent a {HandledObjectName}:\n{message.Content}");
    }

    protected async Task HandleResult(ICommandContext context, string prefix, IResult result)
    {
        await HandleResult(SendMessageAsync, prefix, result);

        async Task SendMessageAsync(string message)
        {
            await context.Channel.SendMessageAsync(message);
        }
    }

    protected async Task HandleResult(Func<string, Task> messageSender, string prefix, IResult result)
    {
        if (result.IsSuccess)
            return;

        var error = result.Error;
        Console.WriteLine(error);

        switch (error)
        {
            case CommandError.UnknownCommand:
                break;

            case CommandError.BadArgCount:
            case CommandError.UnmetPrecondition:
                await messageSender($"{result.ErrorReason}");
                break;

            default:
                await messageSender(error switch
                {
                    // The reason why this help message is not "{prefix}help {command}" is because there is no good way to get the full name of the command
                    CommandError.BadArgCount => $"Unexpected argument count, use `{prefix}help <command>` to get more help regarding this command.",
                    CommandError.ParseFailed => $"Failed to parse the command, use `{prefix}help` to get more help about the available commands.",
                    CommandError.UnmetPrecondition => $"Failed to execute the command, either because this is not for you, or this is not the right place to do it.",

                    CommandError.ObjectNotFound or
                    CommandError.MultipleMatches or
                    CommandError.Exception or
                    CommandError.Unsuccessful => $"Developer is bad, error was caused by his fault.\nError information: {error} - {result.ErrorReason}",

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
        else if (result is ParseResult parseResult)
        {
            Console.WriteLine();
            Console.WriteLine("Parameter Values");
            parseResult.ParamValues?.ForEachObject(Console.WriteLine);

            Console.WriteLine();
            Console.WriteLine("Argument Values");
            parseResult.ArgValues?.ForEachObject(Console.WriteLine);

            Console.WriteLine();
            Console.WriteLine($"Error Parameter: {parseResult.ErrorParameter}\n");
            Console.WriteLine();
        }
    }
}

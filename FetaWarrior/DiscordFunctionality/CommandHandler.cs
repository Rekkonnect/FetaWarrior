using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using FetaWarrior.Configuration;
using FetaWarrior.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality
{
    public class CommandHandler
    {
        public static CommandHandler GlobalCommandHandler { get; private set; }

        public static IEnumerable<CommandInfo> AllAvailableCommands => GlobalCommandHandler.CommandService.Commands;

        public static DiscordSocketClient Client => BotClientManager.Instance.Client;
        public static DiscordRestClient RestClient => BotClientManager.Instance.RestClient;

        static CommandHandler()
        {
            // Nyun-nyun
            GlobalCommandHandler = new(new());
        }

        public CommandService CommandService { get; init; }

        public CommandHandler(CommandService service)
        {
            CommandService = service;
            Initialize();
        }

        #region Initialization
        private void Initialize()
        {
            Task.WaitAll(Task.Run(InitializeCommandHandling));
        }
        private async Task InitializeCommandHandling()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            CommandService.AddTypeReaders(thisAssembly);
            await CommandService.AddModulesAsync(thisAssembly, null);
        }
        #endregion

        public void AddEvents(DiscordSocketClient client)
        {
            client.MessageReceived += HandleCommandAsync;

            // For the time being no commands care about reactions, better not overload the bot
            //Client.ReactionAdded += HandleReactionAddedAsync;
            //Client.ReactionRemoved += HandleReactionRemovedAsync;
        }

        #region Handlers
        private async Task HandleCommandAsync(SocketMessage message)
        {
            var receivedTime = DateTime.Now;

            var socketMessage = message as SocketUserMessage;

            if (!socketMessage?.Author.IsHuman() ?? true)
                return;

            var context = new TimestampedSocketCommandContext(Client, socketMessage, receivedTime);

            var prefix = BotConfig.Instance.GetPrefixForGuild(context.Guild);

            if (!socketMessage.Content.StartsWith(prefix))
                return;

            ConsoleLogging.WriteCurrentTime();
            Console.WriteLine($"{message.Author} sent a command:");
            Console.WriteLine(socketMessage.Content);
            Console.WriteLine();

            var result = await CommandService.ExecuteAsync(context, prefix.Length, null);

            if (!result.IsSuccess)
            {
                var error = result.Error;
                switch (error)
                {
                    case CommandError.UnknownCommand:
                        break;

                    default:
                        await context.Channel.SendMessageAsync(error switch
                        {
                            // The reason why this help message is not "{prefix}help {command}" is because there is no good way to get the full name of the command
                            CommandError.BadArgCount => $"Unexpected argument count, use `{prefix}help <command>` to get more help regarding this command.",
                            CommandError.ParseFailed => $"Failed to parse the command, use `{prefix}help` to get more help about the available commands.",
                            CommandError.UnmetPrecondition => $"This command is not for you.",

                            CommandError.ObjectNotFound or 
                            CommandError.MultipleMatches or 
                            CommandError.Exception or 
                            CommandError.Unsuccessful => $"Developer is bad, error was caused by his fault.\nError information: {error} - {result.ErrorReason}",

                            _ => "Unknown issue occurred.",
                        });
                        break;
                }

                if (result is ExecuteResult executionResult)
                {
                    Console.WriteLine(executionResult.Exception);
                    Console.WriteLine();
                    Console.WriteLine(executionResult.Exception.StackTrace);
                    Console.WriteLine();
                    Console.WriteLine(executionResult.Exception.Message);
                }
                else if (result is ParseResult parseResult)
                {
                    Console.WriteLine("Parameter Values");
                    if (parseResult.ParamValues != null)
                        foreach (var value in parseResult.ParamValues)
                            Console.WriteLine(value);

                    Console.WriteLine();
                    Console.WriteLine("Argument Values");
                    if (parseResult.ArgValues != null)
                        foreach (var value in parseResult.ArgValues)
                            Console.WriteLine(value);

                    Console.WriteLine();
                    Console.WriteLine($"Error Parameter: {parseResult.ErrorParameter}");
                }
            }
        }
        // Future-proofing those functions' existence
        private async Task HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel, SocketReaction reaction)
        {
            // Handle reactions on already active commands
        }
        private async Task HandleReactionRemovedAsync(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel, SocketReaction reaction)
        {
            // Handle reactions on already active commands
        }
        #endregion
    }
}

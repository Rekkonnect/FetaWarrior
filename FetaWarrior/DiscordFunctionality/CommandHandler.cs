using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using FetaWarrior.Configuration;
using FetaWarrior.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality
{
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
            var thisAssembly = Assembly.GetExecutingAssembly();
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

            var prefix = BotConfig.Instance.GetPrefixForGuild(context.Guild);

            int argumentPosition = 0;
            int? customArgumentPosition = null;
            if (socketMessage.HasMentionPrefix(BotClientManager.Instance.Client.CurrentUser, ref argumentPosition))
            {
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

            if (!result.IsSuccess)
            {
                var error = result.Error;
                Console.WriteLine(error);

                switch (error)
                {
                    case CommandError.UnknownCommand:
                        break;

                    case CommandError.BadArgCount:
                    case CommandError.UnmetPrecondition:
                        await context.Channel.SendMessageAsync($"{result.ErrorReason}");
                        break;

                    default:
                        await context.Channel.SendMessageAsync(error switch
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
        #endregion
    }
}

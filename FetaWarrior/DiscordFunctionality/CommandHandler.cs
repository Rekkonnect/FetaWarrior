using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FetaWarrior.Configuration;
using FetaWarrior.Extensions;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality
{
    public class CommandHandler
    {
        public static CommandHandler GlobalCommandHandler { get; private set; }

        public static IEnumerable<CommandInfo> AllAvailableCommands => GlobalCommandHandler.CommandService.Commands;

        public readonly CommandService CommandService;
        public readonly DiscordSocketClient Client;

        public CommandHandler(CommandService service, DiscordSocketClient client)
        {
            CommandService = service;
            Client = client;
            Initialize();
        }

        #region Initialization
        private void Initialize()
        {
            AddEvents();
            Task.WaitAll(Task.Run(InitializeCommandHandling));
        }
        private void AddEvents()
        {
            Client.MessageReceived += HandleCommandAsync;

            // For the time being no commands care about reactions, better not overload the bot
            //Client.ReactionAdded += HandleReactionAddedAsync;
            //Client.ReactionRemoved += HandleReactionRemovedAsync;
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
            var socketMessage = message as SocketUserMessage;

            if (!socketMessage?.Author.IsHuman() ?? true)
                return;

            var context = new SocketCommandContext(Client, socketMessage);
            var prefix = BotConfig.Instance.GetPrefixForGuild(context.Guild.Id);

            if (!socketMessage.Content.StartsWith(prefix))
                return;

            var result = await CommandService.ExecuteAsync(context, prefix.Length, null);

            if (!result.IsSuccess)
            {
                var error = result.Error;
                switch (error)
                {
                    case CommandError.UnknownCommand:
                        return;

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

        public static void InitializeSingletonFromClient(DiscordSocketClient client)
        {
            if (GlobalCommandHandler != null)
                return;

            GlobalCommandHandler = new CommandHandler(new CommandService(), client);
        }
    }
}

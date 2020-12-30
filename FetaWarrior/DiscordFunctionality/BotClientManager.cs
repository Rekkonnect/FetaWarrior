using Discord;
using Discord.Rest;
using Discord.WebSocket;
using FetaWarrior.Configuration;
using System;
using System.Threading.Tasks;
using static System.Console;

namespace FetaWarrior.DiscordFunctionality
{
    public class BotClientManager
    {
        public const GuildPermission MinimumBotPermissions = GuildPermission.KickMembers
                                                           | GuildPermission.BanMembers
                                                           | GuildPermission.ViewChannel
                                                           | GuildPermission.SendMessages
                                                           | GuildPermission.ManageMessages
                                                           | GuildPermission.ReadMessageHistory;

        public static BotClientManager Instance { get; }

        static BotClientManager()
        {
            Instance = new();
        }

        public DiscordSocketClient Client { get; private set; }
        public DiscordRestClient RestClient { get; private set; }

        private BotClientManager()
        {
            StartActivityLoop();
        }

        private void InitializeNewClients()
        {
            InitializeNewSocketClient();
            InitializeNewRestClient();
        }
        private void InitializeNewSocketClient()
        {
            Client?.Dispose();
            Client = new();

            // Class coupling go brrr
            CommandHandler.GlobalCommandHandler.AddEvents(Client);

            Client.Disconnected += ReinitializeSocketClientAfterDisconnection;
            Client.LoggedOut += ReinitializeSocketClientAfterDisconnection;

            Client.LoggedIn += AddSocketClientDisconnectionLoggers;
        }
        private void InitializeNewRestClient()
        {
            RestClient?.Dispose();
            RestClient = new();

            RestClient.LoggedOut += ReinitializeRestClientAfterDisconnection;

            RestClient.LoggedIn += AddRestClientDisconnectionLoggers;
        }

        private async Task AddSocketClientDisconnectionLoggers()
        {
            // Events
            Client.Disconnected -= ReinitializeSocketClientAfterDisconnection;
            Client.LoggedOut -= ReinitializeSocketClientAfterDisconnection;

            Client.Disconnected += LogSocketClientDisconnection;
            Client.LoggedOut += LogSocketClientDisconnection;

            Client.Disconnected += ReinitializeSocketClientAfterDisconnection;
            Client.LoggedOut += ReinitializeSocketClientAfterDisconnection;
        }
        private async Task AddRestClientDisconnectionLoggers()
        {
            // in C#
            RestClient.LoggedOut -= ReinitializeRestClientAfterDisconnection;

            RestClient.LoggedOut += LogRestClientDisconnection;

            RestClient.LoggedOut += ReinitializeRestClientAfterDisconnection;
        }

        public async Task InitializeLogin()
        {
            Task.WaitAll(LoginSocketClient(), LoginRestClient());
        }
        public async Task Logout()
        {
            // are a fucking joke
            Client.Disconnected -= ReinitializeSocketClientAfterDisconnection;
            Client.Disconnected -= LogSocketClientDisconnection;
            Client.LoggedOut -= ReinitializeSocketClientAfterDisconnection;
            Client.LoggedOut -= LogSocketClientDisconnection;
            RestClient.LoggedOut -= ReinitializeRestClientAfterDisconnection;
            RestClient.LoggedOut -= LogRestClientDisconnection;

            Task.WaitAll(LogoutSocketClient(), LogoutRestClient());
        }

        private async Task LoginSocketClient()
        {
            InitializeNewSocketClient();
            await Client.LoginAsync(TokenType.Bot, BotCredentials.Instance.BotToken);
            await Client.StartAsync();

            WriteCurrentTime();
            WriteLine("Socket client logged in");
        }
        private async Task LoginRestClient()
        {
            InitializeNewRestClient();
            await RestClient.LoginAsync(TokenType.Bot, BotCredentials.Instance.BotToken);

            WriteCurrentTime();
            WriteLine("REST client logged in");
        }

        private async Task LogoutSocketClient()
        {
            await Client.StopAsync();
            await Client.LogoutAsync();

            WriteCurrentTime();
            WriteLine("Socket client logged out");
        }
        private async Task LogoutRestClient()
        {
            await RestClient.LogoutAsync();

            WriteCurrentTime();
            WriteLine("REST client logged out");
        }

        #region Client Reinitializers
        private async Task ReinitializeSocketClientAfterDisconnection(Exception e)
        {
            await LoginSocketClient();
        }
        private async Task ReinitializeSocketClientAfterDisconnection()
        {
            await LoginSocketClient();
        }
        private async Task ReinitializeRestClientAfterDisconnection()
        {
            await LoginRestClient();
        }
        #endregion

        #region Disconnection Loggers
        private async Task LogSocketClientDisconnection(Exception e)
        {
            WriteCurrentTime();
            WriteLine("Socket client disconnected");
            WriteException(e);
        }
        private async Task LogSocketClientDisconnection()
        {
            WriteCurrentTime();
            WriteLine("Socket client disconnected");
        }
        private async Task LogRestClientDisconnection()
        {
            WriteCurrentTime();
            WriteLine("REST client disconnected");
        }
        #endregion

        private void StartActivityLoop()
        {
            var lines = new string[]
            {
                $"Default Prefix: \"{BotConfig.DefaultPrefix}\"",
                $"Help Command: \"{BotConfig.DefaultPrefix}help\"",
                $"Servers: {{ServerCount}}",
            };

            RunActivityLoop();

            async Task RunActivityLoop()
            {
                int index = 0;

                while (true)
                {
                    if (Client?.ConnectionState == ConnectionState.Connected)
                    {
                        var line = lines[index].Replace($"{{ServerCount}}", $"{Client.Guilds.Count}");
                        await Client.SetActivityAsync(new Game(line, ActivityType.Playing, details: line));
                    }
                    await Task.Delay(5000);
                    index = (index + 1) % lines.Length;
                }
            }
        }

        private static void WriteCurrentTime()
        {
            Write($"[{DateTime.Now:dd/MM/yyyy HH:mm:ss.ffff}] ");
        }
        private static void WriteException(Exception e)
        {
            WriteLine($"Exception: {e.GetType()}\nMessage: {e.Message ?? "null"}\n\nStack trace:\n{e.StackTrace}");
            if (e.InnerException != null)
                WriteException(e.InnerException);
        }
    }
}

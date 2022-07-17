using Discord;
using Discord.Rest;
using Discord.WebSocket;
using FetaWarrior.Configuration;
using System;
using System.Threading.Tasks;
using static FetaWarrior.ConsoleLogging;

namespace FetaWarrior.DiscordFunctionality
{
    public class BotClientManager
    {
        public const GuildPermission MinimumBotPermissions = GuildPermission.KickMembers
                                                           | GuildPermission.BanMembers
                                                           | GuildPermission.ManageGuild
                                                           | GuildPermission.ViewChannel
                                                           | GuildPermission.SendMessages
                                                           | GuildPermission.ManageMessages
                                                           | GuildPermission.ReadMessageHistory;

        public const GatewayIntents BotIntents = GatewayIntents.DirectMessages
                                               | GatewayIntents.Guilds
                                               | GatewayIntents.GuildMembers
                                               | GatewayIntents.GuildMessages
                                               | GatewayIntents.GuildBans;

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
            DisposeSocketClient().Wait();
            // Nyun-nyun~~
            Client = new(new() { GatewayIntents = BotIntents });

            // Class coupling go brrr
            CommandHandler.GlobalInstance.AddEvents(Client);
            DMHandler.GlobalInstance.AddEvents(Client);

            Client.LoggedIn += AddSocketClientDisconnectionLoggers;
        }
        private void InitializeNewRestClient()
        {
            DisposeRestClient().Wait();
            RestClient = new();

            RestClient.LoggedIn += AddRestClientDisconnectionLoggers;
        }

        private async Task AddSocketClientDisconnectionLoggers()
        {
            Client.Disconnected += LogSocketClientDisconnection;
            Client.LoggedOut += LogSocketClientDisconnection;
        }
        private async Task AddRestClientDisconnectionLoggers()
        {
            RestClient.LoggedOut += LogRestClientDisconnection;
        }

        private void RemoveSocketClientEvents()
        {
            // Events in C#
            if (Client is null)
                return;

            Client.Disconnected -= LogSocketClientDisconnection;
            Client.LoggedOut -= LogSocketClientDisconnection;
        }
        private void RemoveRestClientEvents()
        {
            // are a fucking joke
            if (RestClient is null)
                return;

            RestClient.LoggedOut -= LogRestClientDisconnection;
        }

        private async Task UnsubscribeLogoutSocketClient()
        {
            RemoveSocketClientEvents();
            await LogoutSocketClient();
        }
        private async Task UnsubscribeLogoutRestClient()
        {
            RemoveRestClientEvents();
            await LogoutRestClient();
        }

        private async Task DisposeSocketClient()
        {
            await UnsubscribeLogoutSocketClient();
            Client?.Dispose();
        }
        private async Task DisposeRestClient()
        {
            await UnsubscribeLogoutRestClient();
            RestClient?.Dispose();
        }

        public async Task InitializeLogin()
        {
            Task.WaitAll(LoginSocketClient(), LoginRestClient());
        }
        public async Task Logout()
        {
            Task.WaitAll(UnsubscribeLogoutSocketClient(), UnsubscribeLogoutRestClient());
        }

        private async Task LoginSocketClient()
        {
            InitializeNewSocketClient();
            await Client.LoginAsync(TokenType.Bot, BotCredentials.Instance.BotToken);
            await Client.StartAsync();

            WriteEventWithCurrentTime("Socket client logged in");
        }
        private async Task LoginRestClient()
        {
            InitializeNewRestClient();
            await RestClient.LoginAsync(TokenType.Bot, BotCredentials.Instance.BotToken);

            WriteEventWithCurrentTime("REST client logged in");
        }

        private async Task LogoutSocketClient()
        {
            if (Client is null)
                return;

            await Client.StopAsync();
            await Client.LogoutAsync();

            WriteEventWithCurrentTime("Socket client logged out");
        }
        private async Task LogoutRestClient()
        {
            if (RestClient is null)
                return;

            await RestClient.LogoutAsync();

            WriteEventWithCurrentTime("REST client logged out");
        }

        #region Disconnection Loggers
        private async Task LogSocketClientDisconnection(Exception e)
        {
            WriteEventWithCurrentTime("Socket client disconnected");
            WriteException(e);
        }
        private async Task LogSocketClientDisconnection()
        {
            WriteEventWithCurrentTime("Socket client disconnected");
        }
        private async Task LogRestClientDisconnection()
        {
            WriteEventWithCurrentTime("REST client disconnected");
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
    }
}

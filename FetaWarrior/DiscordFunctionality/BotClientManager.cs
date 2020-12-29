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

        private void InitializeNewClients()
        {
            InitializeNewSocketClient();
            InitializeNewRestClient();
        }
        private void InitializeNewRestClient()
        {
            RestClient?.Dispose();
            RestClient = new();

            // Handles disconnection issues
            RestClient.LoggedOut += HandleRestClientDisconnection;
        }
        private void InitializeNewSocketClient()
        {
            Client?.Dispose();
            Client = new();

            // Class coupling go brrr
            CommandHandler.GlobalCommandHandler.AddEvents(Client);

            // Initiates the status loop
            SetStatus();

            // Handles disconnection issues
            Client.Disconnected += HandleSocketClientDisconnection;
            Client.LoggedOut += HandleSocketClientDisconnection;
        }

        public async Task InitializeLogin()
        {
            Task.WaitAll(LoginSocketClient(), LoginRestClient());

            WriteCurrentTime();
            WriteLine("Logged in.");
        }
        public async Task Logout()
        {
            Client.Disconnected -= HandleSocketClientDisconnection;
            Client.LoggedOut -= HandleSocketClientDisconnection;
            RestClient.LoggedOut -= HandleRestClientDisconnection;

            Task.WaitAll(LogoutSocketClient(), LogoutRestClient());
        }

        private async Task LoginSocketClient()
        {
            InitializeNewSocketClient();
            await Client.LoginAsync(TokenType.Bot, BotCredentials.Instance.BotToken);
            await Client.StartAsync();
            await Client.SetStatusAsync(UserStatus.Online);
        }
        private async Task LoginRestClient()
        {
            InitializeNewRestClient();
            await RestClient.LoginAsync(TokenType.Bot, BotCredentials.Instance.BotToken);
        }

        private async Task LogoutSocketClient()
        {
            await Client.SetStatusAsync(UserStatus.Offline);
            await Client.StopAsync();
            await Client.LogoutAsync();
        }
        private async Task LogoutRestClient()
        {
            await RestClient.LogoutAsync();
        }

        private async Task HandleSocketClientDisconnection(Exception e)
        {
            WriteCurrentTime();
            WriteLine("Socket client disconnected");
            WriteException(e);
            await LoginSocketClient();
        }
        private async Task HandleSocketClientDisconnection()
        {
            WriteCurrentTime();
            WriteLine("Socket client disconnected");
            await LoginSocketClient();
        }
        private async Task HandleRestClientDisconnection()
        {
            WriteCurrentTime();
            WriteLine("REST client disconnected");
            await LoginRestClient();
        }

        private void SetStatus()
        {
            var lines = new string[]
            {
                $"Default Prefix: \"{BotConfig.DefaultPrefix}\"",
                $"Help Command: \"{BotConfig.DefaultPrefix}help\"",
                $"Servers: {{ServerCount}}",
            };

            SetActivityLoop();

            async Task SetActivityLoop()
            {
                int index = 0;

                while (true)
                {
                    var line = lines[index].Replace($"{{ServerCount}}", $"{Client.Guilds.Count}");
                    await Client.SetActivityAsync(new Game(line, ActivityType.Playing, details: line));
                    await Task.Delay(5000);
                    index = (index + 1) % lines.Length;
                }
            }
        }

        private static void WriteCurrentTime()
        {
            Write($"[{DateTime.Now.TimeOfDay}] ");
        }
        private static void WriteException(Exception e)
        {
            WriteLine($"Exception: {e.GetType()}\nMessage: {e.Message ?? "null"}\n\nStack trace:\n{e.StackTrace}");
            if (e.InnerException != null)
                WriteException(e.InnerException);
        }
    }
}

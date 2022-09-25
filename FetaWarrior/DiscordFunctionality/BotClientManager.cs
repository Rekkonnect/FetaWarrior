using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using FetaWarrior.Extensions;
using Garyon.Functions;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static FetaWarrior.ConsoleLogging;

namespace FetaWarrior.DiscordFunctionality;

public class BotClientManager
{
    // ManageGuild for unbanning deleted users
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
                                           | GatewayIntents.GuildVoiceStates
                                           | GatewayIntents.GuildBans;

    public static BotClientManager Instance { get; }

    static BotClientManager()
    {
        Instance = new();
    }

    public bool HasPublicizedComamnds { get; private set; }

    public DiscordSocketClient Client { get; private set; }
    public DiscordRestClient RestClient => Client.Rest;
    //public DiscordRestClient RestClient { get; private set; }

    // Only store an interaction service for the socket client
    public InteractionService InteractionService { get; private set; }

    private BotClientManager()
    {
        StartActivityLoop();
    }

    private async Task RegisterSlashCommandsAsync()
    {
        if (HasPublicizedComamnds)
            return;

        try
        {
            // Yes I have a private server to test the bot on
            const ulong testGuildID = 794554235970125855;

            var entryAssembly = Assembly.GetEntryAssembly();
            InteractionService = new InteractionService(RestClient);
            InteractionService.AddTypeConverters(entryAssembly);

            // Clean the command list from the private server
            await InteractionService.RegisterCommandsToGuildAsync(testGuildID);
            WriteEventWithCurrentTime("Cleaned the leftover commands.");

            var modules = await InteractionService.AddModulesAsync(entryAssembly, null);
            var ownerModules = modules.Where(m => m.Preconditions.Any(precondition => precondition is RequireOwnerAttribute));
            foreach (var ownerModule in ownerModules)
            {
                await InteractionService.RemoveModuleAsync(ownerModule);
                // Owner commands are nerfed; might be the end of an era? 
            }

#if DEBUG && false
            await InteractionService.RegisterCommandsToGuildAsync(testGuildID);
            WriteEventWithCurrentTime("Registered the commands in the test server.");
#else
            await InteractionService.RegisterCommandsGloballyAsync();
            WriteEventWithCurrentTime("Began registering the commands globally.");
#endif

            HasPublicizedComamnds = true;
        }
        catch (Exception e)
        {
            ConsoleUtilities.WriteExceptionInfo(e);
        }
    }

    private void InitializeNewClients()
    {
        InitializeNewSocketClient();
        //InitializeNewRestClient();
    }
    private void InitializeNewSocketClient()
    {
        DisposeSocketClient().Wait();
        // Nyun-nyun~~
        Client = new(new() { GatewayIntents = BotIntents });

        // Class coupling go brrr
        InteractionCommandHandler.GlobalInstance.AddEvents(Client);
        DMHandler.GlobalInstance.AddEvents(Client);

        Client.LoggedIn += OnSocketClientLoggedIn;
        Client.Ready += OnSocketClientReady;
    }
    private void InitializeNewRestClient()
    {
        DisposeRestClient().Wait();
        //RestClient = new();

        RestClient.LoggedIn += AddRestClientDisconnectionLoggers;
    }

    private async Task OnSocketClientReady()
    {
        await RegisterSlashCommandsAsync();
    }
    private async Task OnSocketClientLoggedIn()
    {
        AddSocketClientDisconnectionLoggers();
    }

    private void AddSocketClientDisconnectionLoggers()
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
        await LoginSocketClient();
        return;

        await Task.WhenAll(LoginSocketClient(), LoginRestClient());
    }
    public async Task Logout()
    {
        await UnsubscribeLogoutSocketClient();
        return;

        await Task.WhenAll(UnsubscribeLogoutSocketClient(), UnsubscribeLogoutRestClient());
    }

    private async Task LoginSocketClient()
    {
        InitializeNewSocketClient();
        await Client.LoginAsync(BotCredentials.Instance);
        await Client.StartAsync();

        WriteEventWithCurrentTime("Socket client logged in");
    }
    private async Task LoginRestClient()
    {
        InitializeNewRestClient();
        await RestClient.LoginAsync(BotCredentials.Instance);

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
            // These are becoming obsolete
            /*
            $"Default Prefix: \"{BotPrefixesConfig.DefaultPrefix}\"",
            $"Help Command: \"{BotPrefixesConfig.DefaultPrefix}help\"",
            */

            $"Now supporting slash commands!",
            $"Servers: {{ServerCount}}",
        };

        RunActivityLoop();

        async Task RunActivityLoop()
        {
            int index = 0;

            while (true)
            {
                if (Client?.ConnectionState is ConnectionState.Connected)
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

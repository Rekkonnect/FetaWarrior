using System.Diagnostics;
using System.Threading.Tasks;
using FetaWarrior.Extensions;
using Discord.Interactions;

namespace FetaWarrior.DiscordFunctionality;

public class UtilitiesModule : SocketInteractionModule
{
    #region Repository
    [SlashCommand("repository", "Get the link to this bot's source code repository on GitHub")]
    public async Task ShowRepository()
    {
        await RespondAsync("https://github.com/Rekkonnect/FetaWarrior");
    }
    #endregion

    #region Ping
    [SlashCommand("ping", "Get the current ping")]
    public async Task Ping()
    {
        await RespondAsync($"Current Ping: `{Context.Client.Latency}ms`");
    }

    [SlashCommand("uptime", "Get the current uptime of the bot")]
    public async Task Uptime()
    {
        await RespondAsync($"Uptime: `{Process.GetCurrentProcess().GetElapsedTime()}`");
    }
    #endregion
}

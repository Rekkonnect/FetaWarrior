using System.Diagnostics;
using System.Threading.Tasks;
using FetaWarrior.Extensions;
using Discord.Interactions;

namespace FetaWarrior.DiscordFunctionality;

public class UtilitiesModule : SocketInteractionModule
{
    #region Repository
    [SlashCommand("repository", "Gets the link to this bot's source code repository on GitHub.")]
    public async Task ShowRepositoryAsync()
    {
        await ReplyAsync("https://github.com/Rekkonnect/FetaWarrior");
    }
    #endregion

    #region Ping
    [SlashCommand("ping", "Gets the current ping.")]
    public async Task PingAsync()
    {
        await ReplyAsync($"Current Ping: `{Context.Client.Latency}ms`");
    }

    [SlashCommand("uptime", "Gets the current uptime of the bot.")]
    public async Task UptimeAsync()
    {
        await ReplyAsync($"Uptime: `{Process.GetCurrentProcess().GetElapsedTime()}`");
    }
    #endregion
}

using Discord;
using Discord.Commands;
using FetaWarrior.Configuration;
using FetaWarrior.Extensions;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

public class BotConfigModule : SocketModule
{
    [Command("prefix get")]
    [Alias("prefix")]
    [Summary("Displays the current prefix for this bot on this server.")]
    public async Task DisplayCurrentPrefixAsync()
    {
        await Context.Channel.SendMessageAsync($"The current prefix for this server is {BotConfig.Instance.GetPrefixForChannel(Context.Channel).ToNonFormattableText()}");
    }

    [Command("prefix reset")]
    [Summary("Resets the prefix for this bot on this server to the default one.")]
    [Alias("prefix default")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task ResetPrefixAsync()
    {
        BotConfig.Instance.ResetPrefixForChannel(Context.Channel);
        await Context.Channel.SendMessageAsync($"Reset the current prefix for this server to {BotConfig.DefaultPrefix.ToNonFormattableText()}");
    }

    [Command("prefix set")]
    [Summary("Sets the prefix for this bot on this server to the provided one.")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task SetPrefixAsync
    (
        [Summary("The new prefix to set.")]
        [Remainder]
        string newPrefix
    )
    {
        BotConfig.Instance.SetPrefixForChannel(Context.Channel, newPrefix);
        await Context.Channel.SendMessageAsync($"Changed the current prefix for this server to {newPrefix.ToNonFormattableText()}");
    }
}

using Discord.Commands;
using FetaWarrior.Configuration;
using FetaWarrior.DiscordFunctionality.Attributes;
using FetaWarrior.Extensions;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality
{
    public class BotConfigModule : ModuleBase<SocketCommandContext>
    {
        [Command("prefix")]
        [Summary("Displays the current prefix for this bot on this server.")]
        [RequireContext(ContextType.Guild)]
        [RequireAdminPermission]
        public async Task DisplayCurrentPrefixAsync()
        {
            await Context.Channel.SendMessageAsync($"The current prefix for this server is {BotConfig.Instance.GetPrefixForGuild(Context.Guild).ToNonFormattableText()}");
        }

        [Command("prefix default")]
        [Summary("Resets the prefix for this bot on this server to the default one.")]
        [RequireContext(ContextType.Guild)]
        [RequireAdminPermission]
        public async Task ResetPrefixAsync()
        {
            BotConfig.Instance.ResetPrefixForGuild(Context.Guild);
            await Context.Channel.SendMessageAsync($"Reset the current prefix for this server to {BotConfig.DefaultPrefix.ToNonFormattableText()}");
        }

        [Command("prefix set")]
        [Summary("Sets the prefix for this bot on this server to the provided one.")]
        [RequireContext(ContextType.Guild)]
        [RequireAdminPermission]
        public async Task SetPrefixAsync
        (
            [Name("newPrefix")]
            [Summary("The new prefix to set.")]
            string newPrefix
        )
        {
            BotConfig.Instance.SetPrefixForGuild(Context.Guild, newPrefix);
            await Context.Channel.SendMessageAsync($"Changed the current prefix for this server to {newPrefix.ToNonFormattableText()}");
        }
    }
}

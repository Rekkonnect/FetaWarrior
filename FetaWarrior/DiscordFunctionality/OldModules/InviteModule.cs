using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality.OldModules;

[Obsolete("This aged quite well")]
public class InviteModule : SocketModule
{
    #region Invite
    [Command("invite")]
    [Alias("inv")]
    [Summary("Gets the invite link for this bot.")]
    public async Task InviteAsync()
    {
        await ReplyAsync(InviteUtilities.GenerateBotInviteLink(BotCredentials.Instance.ClientID, (ulong)BotClientManager.MinimumBotPermissions));
    }
    [Command("invite")]
    [Alias("inv")]
    [Summary("Gets the invite link for a bot, requesting admin permissions.")]
    public async Task InviteAsync
    (
        [Summary("The ID of the bot whose invite to request.")]
        ulong botID
    )
    {
        await ReplyAsync(InviteUtilities.GenerateBotInviteLinkAdminPermissions(botID));
    }
    [Command("invite")]
    [Alias("inv")]
    [Summary("Gets the invite link for a bot, requesting the specified permissions.")]
    public async Task InviteAsync
    (
        [Summary("The ID of the bot whose invite to request.")]
        ulong botID,
        [Summary("The permissions integer for the bot. Refer to the Discord API for how to get this number.")]
        ulong permissions
    )
    {
        await ReplyAsync(InviteUtilities.GenerateBotInviteLink(botID, permissions));
    }
    #endregion
}

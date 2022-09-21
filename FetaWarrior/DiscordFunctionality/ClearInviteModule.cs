using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Garyon.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

#nullable enable

[Group("clear-invites", "Clears invites for this guild based on specified criteria")]
[EnabledInDm(false)]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageGuild)]
[RequireBotPermission(GuildPermission.ManageGuild)]
public class ClearInviteModule : SocketInteractionModule
{
    [EnabledInDm(false)]
    [SlashCommand("server", "Clear invites for this server based on specified criteria")]
    public async Task ClearInvitesEntireGuild
    (
        [Summary(description: "Only clear invites that have never been used (defaults to true)")]
        bool neverUsed = true,
        [Summary(description: "Only clear invites that have a user usage limit (defaults to true)")]
        bool withUsageLimit = true,
        [Summary(description: "Only clear invites that have an expiration time (defaults to true)")]
        bool withExpiration = true,
        [Summary(description: "Only clear invites that grant temporary ownership (defaults to false)")]
        bool temporaryOwnership = false
    )
    {
        await ClearInvites(neverUsed, withUsageLimit, withExpiration, temporaryOwnership, null);
    }

    [EnabledInDm(false)]
    [SlashCommand("channel", "Clear invites that target a specific channel based on specified criteria")]
    public async Task ClearInvitesChannel
    (
        [Summary(description: "The channel that the invites target")]
        IGuildChannel channel,
        [Summary(description: "Only clear invites that have never been used (defaults to false)")]
        bool neverUsed = false,
        [Summary(description: "Only clear invites that have a user usage limit (defaults to false)")]
        bool withUsageLimit = false,
        [Summary(description: "Only clear invites that have an expiration time (defaults to false)")]
        bool withExpiration = false,
        [Summary(description: "Only clear invites that grant temporary ownership (defaults to false)")]
        bool temporaryOwnership = false
    )
    {
        await ClearInvites(neverUsed, withUsageLimit, withExpiration, temporaryOwnership, new[] { channel });
    }

    [EnabledInDm(false)]
    [SlashCommand("category", "Clear invites that target any channel inside a specific category based on specified criteria")]
    public async Task ClearInvitesCategory
    (
        [Summary(description: "The category channel whose channels the invites target")]
        ICategoryChannel categoryChannel,
        [Summary(description: "Only clear invites that have never been used (defaults to false)")]
        bool neverUsed = false,
        [Summary(description: "Only clear invites that have a user usage limit (defaults to false)")]
        bool withUsageLimit = false,
        [Summary(description: "Only clear invites that have an expiration time (defaults to false)")]
        bool withExpiration = false,
        [Summary(description: "Only clear invites that grant temporary ownership (defaults to false)")]
        bool temporaryOwnership = false
    )
    {
        var socketCategory = categoryChannel as SocketCategoryChannel;
        await ClearInvites(neverUsed, withUsageLimit, withExpiration, temporaryOwnership, socketCategory!.Channels);
    }

    private async Task ClearInvites
    (
        bool neverUsed,
        bool withUsageLimit,
        bool withExpiration,
        bool temporaryOwnership,
        IEnumerable<IGuildChannel>? channels = null
    )
    {
        await RespondAsync("Getting all the invites for this server...");

        var invites = await Context.Guild.GetInvitesAsync() as IEnumerable<IInviteMetadata>;

        if (neverUsed)
            invites = invites.Where(invite => invite.Uses is 0 or null);

        if (withUsageLimit)
            invites = invites.Where(invite => invite.MaxUses is not null);

        if (withExpiration)
            invites = invites.Where(invite => invite.MaxAge is not null);

        if (temporaryOwnership)
            invites = invites.Where(invite => invite.IsTemporary);

        if (channels is not null)
            invites = invites.Where(invite => channels.Any(c => c.Id == invite.ChannelId));

        var inviteList = invites.ToList();

        if (inviteList.Count is 0)
        {
            await UpdateResponseTextAsync("There were no invites with the specified criteria.");
            return;
        }

        var persistentMessage = new InviteDeletionProgressPersistentMessage(Context.Interaction);
        persistentMessage.Progress.Target = inviteList.Count;
        var messageUpdateTask = persistentMessage.KeepUpdatingProgressMessage(750, true);

        foreach (var invite in inviteList)
        {
            await invite.DeleteAsync();
            persistentMessage.Progress.Current++;
        }

        await messageUpdateTask;
    }

    private sealed class InviteDeletionProgressPersistentMessage : ProgressPersistentMessage
    {
        public InviteDeletionProgressPersistentMessage(IDiscordInteraction interaction)
            : base(interaction)
        {
        }

        public override IActionLexemes Lexemes => throw new System.NotImplementedException();

        private class InviteDeletionLexemes : IActionLexemes
        {
            public static InviteDeletionLexemes Instance { get; } = new();
            private InviteDeletionLexemes() { }

            public string ActionName => "delete";
            public string ActionPastParticiple => "deleted";
            public string ObjectName => "invite";
            public string ObjectNamePlural => "invites";
        }
    }
}
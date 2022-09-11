using Discord;
using Discord.Interactions;
using FetaWarrior.DiscordFunctionality.Slash.Attributes;
using FetaWarrior.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

[RequireGuildContext]
[RequireUserPermission(GuildPermission.ManageGuild)]
[RequireBotPermission(GuildPermission.ManageGuild)]
public class UnbanDeletedUsersModule : MassYeetUsersModuleBase
{
    public override UserYeetingLexemes Lexemes => MassUnbanningLexemes.Instance;

    [SlashCommand("unban-deleted", "Revokes the ban for all banned accounts that have been deleted as deleted.")]
    public async Task UnbanAllDeleted()
    {
        var guild = Context.Guild;

        var originalProgressMessage = await Context.Channel.SendMessageAsync("Getting this server's ban list...");
        var persistentProgressMessage = new PersistentMessage(originalProgressMessage);

        var bans = await guild.GetAllBansAsync();

        await persistentProgressMessage.SetContentAsync("Detecting possibly deleted accounts whose bans to revoke...");

        await MassYeetWithProgress(bans.Where(ban => ban.User.IsDeleted()).Select(b => b.User.Id).ToArray(), persistentProgressMessage);
    }

    protected override Task YeetUser(ulong userID, string reason)
    {
        return Context.Guild.RemoveBanAsync(userID, new() { AuditLogReason = reason });
    }

    private sealed class MassUnbanningLexemes : UserYeetingLexemes
    {
        public static MassUnbanningLexemes Instance { get; } = new();
        private MassUnbanningLexemes() { }

        public override string ActionName => "unban";
        public override string ActionPastParticiple => "unbanned";
    }
}

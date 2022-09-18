using Discord;
using Discord.Interactions;
using FetaWarrior.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

[EnabledInDm(false)]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageGuild)]
[RequireBotPermission(GuildPermission.ManageGuild)]
public class UnbanDeletedUsersModule : MassYeetUsersModuleBase
{
    public override UserYeetingLexemes Lexemes => MassUnbanningLexemes.Instance;

    [EnabledInDm(false)]
    [SlashCommand("unban-deleted", "Revoke the ban for all banned accounts that have been detected as deleted")]
    public async Task UnbanAllDeleted()
    {
        var guild = Context.Guild;

        await RespondAsync("Getting this server's ban list...");

        var bans = await guild.GetAllBansAsync();

        await UpdateResponseTextAsync("Detecting possibly deleted accounts whose bans to revoke...");

        await MassYeetWithProgress(bans.Where(ban => ban.User.IsDeleted()).Select(ban => ban.User).ToArray());
    }

    protected override Task YeetUser(IUser user, string reason)
    {
        return Context.Guild.RemoveBanAsync(user, new() { AuditLogReason = reason });
    }

    private sealed class MassUnbanningLexemes : UserYeetingLexemes
    {
        public static MassUnbanningLexemes Instance { get; } = new();
        private MassUnbanningLexemes() { }

        public override string ActionName => "unban";
        public override string ActionPastParticiple => "unbanned";
    }
}

using Discord;
using Discord.Commands;
using FetaWarrior.DiscordFunctionality.Attributes;
using FetaWarrior.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

[RequireGuildContext]
[RequireUserPermission(GuildPermission.ManageGuild)]
[RequireBotPermission(GuildPermission.ManageGuild)]
public class UnbanDeletedUsersModule : MassYeetUsersModuleBase
{
    public override UserYeetingLexemes Lexemes => new MassUnbanningLexemes();

    [Command("unban deleted")]
    [Alias("unban del", "unband")]
    [Summary("Revokes the ban for all banned accounts that have been deleted. Can be used to reduce clutter in the ban list.")]
    [Remarks(@"The deleted user detection may not be entirely accurate; it depends on the username of the deleted account matching the ""Deleted User xxxxxxxx"" pattern.")]
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
        public override string ActionName => "unban";
        public override string ActionPastParticiple => "unbanned";
    }
}

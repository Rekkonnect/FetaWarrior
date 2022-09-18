using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

[Group("massban", "Mass ban all users that suit a specified filter")]
[EnabledInDm(false)]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.BanMembers)]
[RequireBotPermission(GuildPermission.BanMembers)]
public class MassBanModule : MassYeetUsersModuleBase
{
    public override UserYeetingLexemes Lexemes => MassBanningLexemes.Instance;

    [SlashCommand("server-message", "Mass ban all users that are greeted with server messages within a range")]
    public async Task MassBanFromServerMessages
    (
        [Summary(description: "The ID of the first server message, inclusive")]
        Snowflake firstMessageID,
        [Summary(description: "The ID of the last server message, inclusive")]
        Snowflake lastMessageID = default,
        [Summary(description: "Enable this to only ban users with a default avatar")]
        bool defaultAvatarOnly = false
    )
    {
        await MassYeetFromServerMessages(firstMessageID, lastMessageID, defaultAvatarOnly);
    }
    [SlashCommand("join-date", "Mass ban all users that joined within the range of two users' join dates")]
    public async Task MassBanFromJoinDate
    (
        [Summary(description: "The user whose join date is the starting point, inclusive")]
        IGuildUser firstUser,
        [Summary(description: "The user whose join date is the ending point, inclusive (omitting implies up until now)")]
        IGuildUser lastUser = null,
        [Summary(description: "Enable this to only ban users with a default avatar")]
        bool defaultAvatarOnly = false
    )
    {
        await MassYeetFromJoinDate(firstUser, lastUser, defaultAvatarOnly);
    }

    protected override async Task YeetUser(IUser user, string reason) => await Context.Guild.AddBanAsync(user, 7, reason);

    private sealed class MassBanningLexemes : UserYeetingLexemes
    {
        public static MassBanningLexemes Instance { get; } = new();
        private MassBanningLexemes() { }

        public override string ActionName => "ban";
        public override string ActionPastParticiple => "banned";
    }
}

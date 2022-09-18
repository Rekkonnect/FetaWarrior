using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

[Group("masskick", "Mass kicks all users that suit a specified filter")]
[EnabledInDm(false)]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.KickMembers)]
[RequireBotPermission(GuildPermission.KickMembers)]
public class MassKickModule : MassYeetUsersModuleBase
{
    public override UserYeetingLexemes Lexemes => UserKickingLexemes.Instance;

    [SlashCommand("server-message", "Mass kick all users that are greeted with server messages within a range")]
    public async Task MassKickFromServerMessages
    (
        [Summary(description: "The ID of the first server message, inclusive")]
        Snowflake firstMessageID,
        [Summary(description: "The ID of the last server message, inclusive")]
        Snowflake lastMessageID = default,
        [Summary(description: "Enable this to only kick users with a default avatar")]
        bool defaultAvatarOnly = false
    )
    {
        await MassYeetFromServerMessages(firstMessageID, lastMessageID, defaultAvatarOnly);
    }
    [SlashCommand("join-date", "Mass kick all users that joined within the range of two users' join dates")]
    public async Task MassKickFromJoinDate
    (
        [Summary(description: "The user whose join date is the starting point, inclusive")]
        IGuildUser firstUser,
        [Summary(description: "The user whose join date is the ending point, inclusive (omitting implies up until now)")]
        IGuildUser lastUser = null,
        [Summary(description: "Enable this to only kick users with a default avatar")]
        bool defaultAvatarOnly = false
    )
    {
        await MassYeetFromJoinDate(firstUser, lastUser, defaultAvatarOnly);
    }

    protected override async Task YeetUser(IUser user, string reason)
    {
        if (user is not IGuildUser guildUser)
            return;

        await guildUser.KickAsync(reason);
    }

    private sealed class UserKickingLexemes : UserYeetingLexemes
    {
        public static UserKickingLexemes Instance { get; } = new();
        private UserKickingLexemes() { }

        public override string ActionName => "kick";
        public override string ActionPastParticiple => "kicked";
    }
}

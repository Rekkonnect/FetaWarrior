using Discord;
using Discord.Interactions;
using FetaWarrior.DiscordFunctionality.Slash.Attributes;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

[Group("massban", "Mass bans all users that suit a specified filter.")]
[RequireGuildContext]
[RequireUserPermission(GuildPermission.BanMembers)]
[RequireBotPermission(GuildPermission.BanMembers)]
public class MassBanModule : MassYeetUsersModuleBase
{
    public override UserYeetingLexemes Lexemes => MassBanningLexemes.Instance;

    #region Server Messages
    [SlashCommand("server-message", "Mass ban all users that are greeted with server messages within a range.")]
    public async Task MassBanFromServerMessages
    (
        [Summary(description: "The ID of the first server message, inclusive.")]
        Snowflake firstMessageID,
        [Summary(description: "The ID of the last server message, inclusive.")]
        Snowflake lastMessageID = default
    )
    {
        await MassYeetFromServerMessages(firstMessageID, lastMessageID);
    }
    #endregion
    #region Join Date
    [SlashCommand("join-date", "Mass ban all users that joined within the range of two users' join dates.")]
    public async Task MassBanFromJoinDate
    (
        [Summary(description: "The user whose join date is the starting point, inclusive.")]
        IGuildUser firstUser,
        [Summary(description: "The user whose join date is the ending point, inclusive. Omitting implies up until now.")]
        IGuildUser lastUser = null
    )
    {
        await MassYeetFromJoinDate(firstUser, lastUser);
    }
    #endregion

    protected override async Task YeetUser(ulong userID, string reason) => await Context.Guild.AddBanAsync(userID, 7, reason);

    private sealed class MassBanningLexemes : UserYeetingLexemes
    {
        public static MassBanningLexemes Instance { get; } = new();
        private MassBanningLexemes() { }

        public override string ActionName => "ban";
        public override string ActionPastParticiple => "banned";
    }
}

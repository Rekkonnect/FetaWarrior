using Discord;
using Discord.Interactions;
using FetaWarrior.DiscordFunctionality.Slash.Attributes;
using FetaWarrior.DiscordFunctionality.Utilities;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

[Group("masskick", "Mass kicks all users that suit a specified filter.")]
[RequireGuildContext]
[RequireUserPermission(GuildPermission.KickMembers)]
[RequireBotPermission(GuildPermission.KickMembers)]
public class MassKickModule : MassYeetUsersModuleBase
{
    public override UserYeetingLexemes Lexemes => UserKickingLexemes.Instance;

    #region Server Messages
    [SlashCommand("server-message", "Mass kick all users that are greeted with server messages within a range.")]
    public async Task MassKickFromServerMessages
    (
        [Summary(description: "The ID of the first server message, inclusive.")]
        ulong firstMessageID,
        [Summary(description: "The ID of the last server message, inclusive.")]
        ulong lastMessageID = Snowflakes.LargeSnowflake
    )
    {
        await MassYeetFromServerMessages(firstMessageID, lastMessageID);
    }
    #endregion
    #region Join Date
    [SlashCommand("join-date", "Mass kick all users that joined within the range of two users' join dates.")]
    public async Task MassKickFromJoinDate
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

    protected override async Task YeetUser(ulong userID, string reason)
    {
        var guild = await BotClientManager.Instance.RestClient.GetGuildAsync(Context.Guild.Id);
        var user = await guild.GetUserAsync(userID);
        await user.KickAsync(reason);
    }

    private sealed class UserKickingLexemes : UserYeetingLexemes
    {
        public static UserKickingLexemes Instance { get; } = new();
        private UserKickingLexemes() { }

        public override string ActionName => "kick";
        public override string ActionPastParticiple => "kicked";
    }
}

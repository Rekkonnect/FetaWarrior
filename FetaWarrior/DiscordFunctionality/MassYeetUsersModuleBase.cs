using Discord;
using Discord.Net;
using FetaWarrior.Extensions;
using FetaWarrior.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

public abstract class MassYeetUsersModuleBase : SocketInteractionModule
{
    public abstract UserYeetingLexemes Lexemes { get; }

    #region Server Messages
    protected async Task MassYeetFromServerMessages(Snowflake firstMessageID, Snowflake lastMessageID)
    {
        if (lastMessageID.Value is 0)
            lastMessageID = Snowflake.LargeSnowflake;

        var guild = Context.Guild;
        var channel = guild.SystemChannel;

        await RespondAsync($"Discovering users to {Lexemes.ActionName}... 0 users have been found so far.");
        var persistentMessage = new MassYeetingProgressPersistentMessage(this);
        var discoveryCompleteBoxed = new BoxedStruct<bool>();
        var updateTask = persistentMessage.KeepUpdatingDiscoveryMessage(750, discoveryCompleteBoxed);

        var messageRange = await channel.GetMessageRangeAsync(firstMessageID, lastMessageID, IsGuildMemberJoinSystemMessage, persistentMessage.Progress);
        discoveryCompleteBoxed.Value = true;

        var toYeet = messageRange.Select(sm => sm.Author.Id).ToArray();

        await MassYeetWithProgress(toYeet);

        // Functions
        static bool IsGuildMemberJoinSystemMessage(IMessage m) => m.Type == MessageType.GuildMemberJoin;
    }
    #endregion
    #region Join Date
    private async Task<IGuildUser> GetLastJoinedUser()
    {
        var guild = Context.Guild;

        // Ensure that all users are downloaded
        await guild.DownloadUsersAsync();

        var lastJoinedUser = guild.Users.First();
        foreach (var user in guild.Users)
            if (user.JoinedAt > lastJoinedUser.JoinedAt)
                lastJoinedUser = user;

        return lastJoinedUser;
    }

    public async Task MassYeetFromJoinDate(IGuildUser firstUser, IGuildUser lastUser)
    {
        var restGuild = await BotClientManager.Instance.RestClient.GetGuildAsync(Context.Guild.Id);

        lastUser ??= await GetLastJoinedUser();

        await RespondAsync("Retrieving guild member list...");

        var users = await restGuild.GetUsersAsync().FlattenAsync();

        await UpdateResponseTextAsync($"Discovering users to {Lexemes.ActionName}...");

        var toBan = users.Where(u => u.JoinedAt >= firstUser.JoinedAt && u.JoinedAt <= lastUser.JoinedAt).Select(u => u.Id).ToArray();

        await MassYeetWithProgress(toBan);
    }
    #endregion
    #region Default Avatar
    public async Task MassYeetFromDefaultAvatar(IGuildUser firstUser, IGuildUser lastUser)
    {
        var restGuild = await BotClientManager.Instance.RestClient.GetGuildAsync(Context.Guild.Id);

        await RespondAsync("Retrieving guild member list...");

        var users = await restGuild.GetUsersAsync().FlattenAsync();

        await UpdateResponseTextAsync($"Discovering users to {Lexemes.ActionName}...");

        var toBan = users.Where(u => u.GetAvatarUrl() is null && u.JoinedAt >= firstUser.JoinedAt && u.JoinedAt <= lastUser.JoinedAt).Select(u => u.Id).ToArray();

        await MassYeetWithProgress(toBan);
    }
    #endregion

    // TODO: Change `ulong userID` to `IGuildUser user`
    protected abstract Task YeetUser(ulong userID, string reason);

    // TODO: Change to accept ICollection<IGuildUser>
    protected async Task MassYeetWithProgress(ICollection<ulong> toYeet)
    {
        int yeetedUserCount = 0;
        int forbiddenOperationCount = 0;
        bool yeetingComplete = false;
        var progressUpdatingTask = UpdateYeetingProgress();

        foreach (ulong userID in toYeet)
        {
            bool success = false;
            while (!success)
            {
                success = true;
                try
                {
                    await YeetUser(userID, $"Mass {Lexemes.ActionPastParticiple}");
                }
                catch (HttpException e) when (e.HttpCode == HttpStatusCode.Forbidden)
                {
                    forbiddenOperationCount++;
                }
                catch
                {
                    // In case of any other error, attempt to retry
                    success = false;
                }
            }
            yeetedUserCount++;
        }

        // THIS SUGGESTION IS A FALSE POSITIVE; This matters for breaking the async loop
        // Though in a fairer note, this might be a general suggestion to avoid this practice
#pragma warning disable IDE0059
        yeetingComplete = true;
#pragma warning restore IDE0059

        await progressUpdatingTask;

        async Task UpdateYeetingProgress()
        {
            while (!yeetingComplete)
            {
                var progressMessageContent = $"{toYeet.Count} users are being {Lexemes.ActionPastParticiple}... {yeetedUserCount - forbiddenOperationCount} users have been {Lexemes.ActionPastParticiple} so far.";
                if (forbiddenOperationCount > 0)
                    progressMessageContent += $"\n{forbiddenOperationCount} users could not be {Lexemes.ActionPastParticiple}.";

                await UpdateResponseTextAsync(progressMessageContent);
                await Task.Delay(1000);
            }

            var finalizedMessage = $"{toYeet.Count - forbiddenOperationCount} users have been {Lexemes.ActionPastParticiple}.";
            if (forbiddenOperationCount > 0)
                finalizedMessage += $"\n{forbiddenOperationCount} users could not be {Lexemes.ActionPastParticiple}.";

            await UpdateResponseTextAsync(finalizedMessage);
        }
    }

    public abstract class UserYeetingLexemes : IActionLexemes
    {
        public string ObjectName => "user";
        public string ObjectNamePlural => "users";

        public abstract string ActionName { get; }
        public abstract string ActionPastParticiple { get; }
    }
}

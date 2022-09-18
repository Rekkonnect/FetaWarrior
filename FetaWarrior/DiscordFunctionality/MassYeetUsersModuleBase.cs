using Discord;
using Discord.Net;
using Discord.Rest;
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
    protected async Task MassYeetFromServerMessages(Snowflake firstMessageID, Snowflake lastMessageID, bool defaultAvatarOnly)
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

        var toYeet = messageRange
            .Select(sm => sm.Author)
            .Where(ShouldYeet)
            .ToArray();

        await MassYeetWithProgress(toYeet);

        // Functions
        static bool IsGuildMemberJoinSystemMessage(IMessage m)
        {
            return m.Type is MessageType.GuildMemberJoin;
        }

        bool ShouldYeet(IUser user)
        {
            return ShouldYeetDefaultAvatarFilter(user, defaultAvatarOnly);
        }
    }
    #endregion
    #region Join Date
    private static IGuildUser LastJoinedUser(IEnumerable<IGuildUser> users)
    {
        return users.MaxBy(user => user.JoinedAt);
    }

    private async Task<RestGuild> GetRestGuildAsync()
    {
        return await BotClientManager.Instance.RestClient.GetGuildAsync(Context.Guild.Id);
    }

    protected async Task MassYeetFromJoinDate(IGuildUser firstUser, IGuildUser lastUser, bool defaultAvatarOnly)
    {
        var restGuild = await GetRestGuildAsync();

        await RespondAsync("Retrieving guild member list...");

        var users = await restGuild.GetUsersAsync().FlattenAsync();
        lastUser ??= LastJoinedUser(users);

        await UpdateResponseTextAsync($"Discovering users to {Lexemes.ActionName}...");

        var toYeet = users.Where(JoinedBetween).ToArray();

        await MassYeetWithProgress(toYeet);

        bool JoinedBetween(IGuildUser user)
        {
            if (!ShouldYeetDefaultAvatarFilter(user, defaultAvatarOnly))
                return false;

            return user.JoinedAt >= firstUser.JoinedAt && user.JoinedAt <= lastUser.JoinedAt;
        }
    }

    private static bool ShouldYeetDefaultAvatarFilter(IUser user, bool defaultAvatarOnly)
    {
        return !defaultAvatarOnly
            || user.AvatarId is null;
    }
    #endregion

    protected abstract Task YeetUser(IUser user, string reason);

    // TODO: Use Progress
    protected async Task MassYeetWithProgress(ICollection<IUser> toYeet)
    {
        int yeetedUserCount = 0;
        int forbiddenOperationCount = 0;
        bool yeetingComplete = false;
        var progressUpdatingTask = UpdateYeetingProgress();

        foreach (var user in toYeet)
        {
            bool success = false;
            while (!success)
            {
                success = true;
                try
                {
                    await YeetUser(user, $"Mass {Lexemes.ActionPastParticiple}");
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
                await Task.Delay(750);
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

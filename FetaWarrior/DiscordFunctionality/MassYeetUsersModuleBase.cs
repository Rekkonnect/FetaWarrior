using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.Rest;
using FetaWarrior.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality
{
    public abstract class MassYeetUsersModuleBase : ModuleBase<SocketCommandContext>
    {
        protected abstract string YeetAction { get; }
        protected abstract string YeetActionPastParticiple { get; }

        #region Server Messages
        protected async Task MassYeetFromServerMessages(ulong firstMessageID)
        {
            var lastMessageID = (await Context.Channel.GetLastMessageAsync()).Id;
            await MassYeetFromServerMessages(firstMessageID, lastMessageID);
        }
        protected async Task MassYeetFromServerMessages(ulong firstMessageID, ulong lastMessageID)
        {
            var guild = Context.Guild;
            var channel = guild.SystemChannel;

            var originalProgressMessage = await Context.Channel.SendMessageAsync($"Discovering users to {YeetAction}... 0 users have been found so far.");
            var persistentProgressMessage = new PersistentMessage(originalProgressMessage);

            var toYeet = (await channel.GetMessageRangeAsync(firstMessageID, lastMessageID, IsGuildMemberJoinSystemMessage, UpdateMessage)).Select(sm => sm.Author.Id).ToArray();

            await MassYeetWithProgress(toYeet, persistentProgressMessage);

            // Functions
            static bool IsGuildMemberJoinSystemMessage(IMessage m) => m.Type == MessageType.GuildMemberJoin;

            async Task UpdateMessage(int messages)
            {
                await persistentProgressMessage.SetContentAsync($"Discovering users to {YeetAction}... {messages} users have been found so far.");
            }
        }
        #endregion
        #region Join Date
        public async Task MassYeetFromJoinDate(ulong firstUserID)
        {
            var guild = Context.Guild;

            // Ensure that all users are downloaded
            await guild.DownloadUsersAsync();

            var lastJoinedUser = guild.Users.First();
            foreach (var user in guild.Users)
                if (user.JoinedAt > lastJoinedUser.JoinedAt)
                    lastJoinedUser = user;

            await MassYeetFromJoinDate(firstUserID, lastJoinedUser.Id);
        }
        public async Task MassYeetFromJoinDate(ulong firstUserID, ulong lastUserID)
        {
            var restGuild = await BotClientManager.Instance.RestClient.GetGuildAsync(Context.Guild.Id);

            var originalProgressMessage = await Context.Channel.SendMessageAsync("Retrieving guild member list...");
            var persistentProgressMessage = new PersistentMessage(originalProgressMessage);

            var users = await restGuild.GetUsersAsync().FlattenAsync();

            await persistentProgressMessage.SetContentAsync($"Discovering users to {YeetAction}...");

            var firstUser = users.FirstOrDefault(u => u.Id == firstUserID);
            if (firstUser == null)
            {
                await persistentProgressMessage.SetContentAsync($"The first user ID {firstUserID} could not be found in this server.");
                return;
            }

            var lastUser = users.FirstOrDefault(u => u.Id == lastUserID);
            if (lastUser == null)
            {
                await persistentProgressMessage.SetContentAsync($"The last user ID {lastUserID} could not be found in this server.");
                return;
            }

            var toBan = users.Where(u => u.JoinedAt >= firstUser.JoinedAt && u.JoinedAt <= lastUser.JoinedAt).Select(u => u.Id).ToArray();

            await MassYeetWithProgress(toBan, persistentProgressMessage);
        }
        #endregion

        protected abstract Task YeetUser(ulong userID, string reason);

        private async Task MassYeetWithProgress(ICollection<ulong> toYeet, PersistentMessage persistentProgressMessage)
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
                        await YeetUser(userID, $"Mass {YeetActionPastParticiple}");
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

            yeetingComplete = true;
            await progressUpdatingTask;

            async Task UpdateYeetingProgress()
            {
                while (!yeetingComplete)
                {
                    var progressMessageContent = $"{toYeet.Count} users are being {YeetActionPastParticiple}... {yeetedUserCount - forbiddenOperationCount} users have been {YeetActionPastParticiple} so far.";
                    if (forbiddenOperationCount > 0)
                        progressMessageContent += $"\n{forbiddenOperationCount} users could not be {YeetActionPastParticiple}.";

                    await persistentProgressMessage.SetContentAsync(progressMessageContent);
                    await Task.Delay(1000);
                }

                var finalizedMessage = $"{toYeet.Count - forbiddenOperationCount} users have been {YeetActionPastParticiple}.";
                if (forbiddenOperationCount > 0)
                    finalizedMessage += $"\n{forbiddenOperationCount} users could not be {YeetActionPastParticiple}.";

                await persistentProgressMessage.SetContentAsync(finalizedMessage);
            }
        }
    }
}

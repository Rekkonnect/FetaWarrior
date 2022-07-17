using Discord;
using Discord.Commands;
using Discord.Rest;
using FetaWarrior.DiscordFunctionality.Attributes;
using FetaWarrior.Extensions;
using Garyon.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality
{
    using static Utilities.Snowflakes;

    [RequireGuildContext]
    [RequireUserPermission(ChannelPermission.ManageMessages, Group = "User")]
    [RequireUserPermission(GuildPermission.ManageMessages, Group = "User")]
    [RequireBotPermission(ChannelPermission.ManageMessages, Group = "Bot")]
    [RequireBotPermission(GuildPermission.ManageMessages, Group = "Bot")]
    public class MessageHandlingModule : SocketModule
    {
        #region Delete All
        [Command("delete all")]
        [Alias("remove all", "clear all")]
        [Summary("Deletes all messages that were sent in the channel that the command was sent in.")]
        public async Task DeleteAllMessages()
        {
            await DeleteAllMessages(Context.Channel as ITextChannel);
        }
        [Command("delete all")]
        [Alias("remove all", "clear all")]
        [Summary("Deletes all messages that were sent in the specified channel.")]
        public async Task DeleteAllMessages
        (
            [Summary("The channel on which the the messages to delete are contained.")]
            ITextChannel textChannel
        )
        {
            bool valid = await ValidateChannel(textChannel);
            if (!valid)
                return;

            var lastMessage = await textChannel.GetLastMessageAsync();
            
            if (lastMessage is null)
            {
                await Context.Channel.SendMessageAsync("The channel contains no messages.");
                return;
            }

            await DeleteOtherChannelMessages(textChannel, 0, lastMessage.Id);
        }
        #endregion

        #region Delete This Channel Messages
        [Command("delete here")]
        [Alias("remove here", "clear here")]
        [Summary("Deletes all messages from this channel that were sent after the provided message, **including** the first message. Only deletes messages in this channel.")]
        public async Task DeleteThisChannelMessages
        (
            [Summary("The ID of the first message that will be deleted, **inclusive**.")]
            ulong firstMessageID
        )
        {
            await DeleteOtherChannelMessages(Context.Channel as ITextChannel, firstMessageID);
        }
        [Command("delete here")]
        [Alias("remove here", "clear here")]
        [Summary("Deletes all messages from a specified channel that were sent within the provided message range, **including** the first and the last messages. Only deletes messages in that same channel.")]
        public async Task DeleteThisChannelMessages
        (
            [Summary("The ID of the first message that will be deleted, **inclusive**.")]
            ulong firstMessageID,
            [Summary("The ID of the last message that will be deleted, **inclusive**.")]
            ulong lastMessageID
        )
        {
            await DeleteOtherChannelMessages(Context.Channel as ITextChannel, firstMessageID, lastMessageID);
        }
        #endregion

        #region Delete Other Channel Messages
        [Command("delete oc")]
        [Alias("remove oc", "clear oc")]
        [Summary("Deletes all messages from a specified channel that were sent after the provided message, **including** the first message. Only deletes messages in that same channel.")]
        public async Task DeleteOtherChannelMessages
        (
            [Summary("The channel on which the the messages to delete are contained.")]
            ITextChannel textChannel,
            [Summary("The ID of the first message that will be deleted, **inclusive**.")]
            ulong firstMessageID
        )
        {
            var lastMessageID = (await textChannel.GetLastMessageAsync()).Id;
            await DeleteOtherChannelMessages(textChannel, firstMessageID, lastMessageID);
        }
        [Command("delete oc")]
        [Alias("remove oc", "clear oc")]
        [Summary("Deletes all messages from a specified channel that were sent within the provided message range, **including** the first and the last messages. Only deletes messages in that same channel.")]
        public async Task DeleteOtherChannelMessages
        (
            [Summary("The channel on which the the messages to delete are contained.")]
            ITextChannel textChannel,
            [Summary("The ID of the first message that will be deleted, **inclusive**.")]
            ulong firstMessageID,
            [Summary("The ID of the last message that will be deleted, **inclusive**.")]
            ulong lastMessageID
        )
        {
            var contextChannel = Context.Channel;

            bool valid = await ValidateIDs(firstMessageID, lastMessageID);
            if (!valid)
                return;

            valid = await ValidateChannel(textChannel);
            if (!valid)
                return;

            var progressMessageTimestamp = Context.Message.Timestamp;
            var persistentProgressMessage = new MessageDeletingProgressPersistentMessage(contextChannel);

            var foundMessages = await DiscoverMessagesAsync(DiscoverFilteredMessagesAsync, persistentProgressMessage);

            async Task<HashSet<IMessage>> DiscoverFilteredMessagesAsync()
            {
                return await textChannel.GetMessageRangeAsync(firstMessageID, lastMessageID, value => persistentProgressMessage.Progress.Target = value);
            }

            var updateTask = persistentProgressMessage.KeepUpdatingProgressMessage(750, true, 5000);
            
            await textChannel.DeleteFoundMessages(foundMessages, progressMessageTimestamp.UtcDateTime, persistentProgressMessage.Progress);

            await updateTask;
        }
        #endregion

        #region Delete Deleted Announcement Messages
        //                              DD/MM/YYYY
        // The article was published in 11/11/2020, but we're adding an estimated extra month for beta rollout
        // In the event that we get followed announcement messages prior to that date, the users should file an issue to fix this
        private static readonly DateTime earliestAnnouncementDate = new(2020, 10, 10);
        private const string earliestAnnouncementDateString = "10/10/2020";
        private const string daCommandRemarkStatement0 = $@"The messages are filtered to dates not prior to {earliestAnnouncementDateString}, which the current estimate of the earliest announcement messages.";
        private const string daCommandRemarkStatement1 = $@"Please report messages uncaught by this filter.";
        private const string daCommandRemarkStatement2 = $@"Search for ""[Original Message Deleted]"" after every operation to guarantee no leftovers.";
        private const string daCommandRemark =
$@"{daCommandRemarkStatement0} {daCommandRemarkStatement1} {daCommandRemarkStatement2}";

        // TODO: Build indexing (database moment)
        #region Delete Server-Wide Deleted Announcement Messages
        [Command("delete da all")]
        [Alias("remove da all", "clear da all")]
        [Summary("Deletes all system-sent announcement messages that were deleted from the original source from all channels in this server.")]
        [Remarks(daCommandRemark)]
        public async Task DeleteServerWideDeletedAnnouncementMessages()
        {
            await DeleteServerWideDeletedAnnouncementMessages(0, LargeSnowflake);
        }
        [Command("delete da all")]
        [Alias("remove da all", "clear da all")]
        [Summary("Deletes all system-sent announcement messages that were deleted from the original source from all channels in this server that were sent after the provided message, **including** the first message.")]
        [Remarks(daCommandRemark)]
        public async Task DeleteServerWideDeletedAnnouncementMessages
        (
            [Summary("The ID of the first announcement message that will be deleted, **inclusive**.")]
            ulong firstMessageID
        )
        {
            await DeleteServerWideDeletedAnnouncementMessages(firstMessageID, LargeSnowflake);
        }
        [Command("delete da all")]
        [Alias("remove da all", "clear da all")]
        [Summary("Deletes all system-sent announcement messages that were deleted from the original source from all channels in this server that were sent within the provided message range, **including** the first and the last messages.")]
        [Remarks(daCommandRemark)]
        public async Task DeleteServerWideDeletedAnnouncementMessages
        (
            [Summary("The ID of the first announcement message that will be deleted, **inclusive**.")]
            ulong firstMessageID,
            [Summary("The ID of the last announcement message that will be deleted, **inclusive**. Must be greater than 0 and not less than the first message ID.")]
            ulong lastMessageID
        )
        {
            var contextChannel = Context.Channel;

            bool valid = await ValidateIDs(firstMessageID, lastMessageID);
            if (!valid)
                return;

            var persistentProgressMessage = new MessageDeletingProgressPersistentMessage(contextChannel);

            // TODO: Abstract this logic away to another component
            var textChannels = Context.Guild.TextChannels;
            var messageRetrievalTasks = new Task<HashSet<IMessage>>[textChannels.Count];
            var retrievedMessageCounts = new int[textChannels.Count];

            var completedBoxed = new BoxedStruct<bool>();
            var discoveryUpdate = persistentProgressMessage.KeepUpdatingDiscoveryMessage(750, completedBoxed);

            foreach (var (index, textChannel) in textChannels.WithIndex())
            {
                var task = textChannel.GetMessageRangeAsync(firstMessageID, lastMessageID, earliestAnnouncementDate,
                    IsSourceMessageDeleted, UpdateRetrievedMessageCounts);
                messageRetrievalTasks[index] = task;

                void UpdateRetrievedMessageCounts(int value)
                {
                    retrievedMessageCounts[index] = value;
                    persistentProgressMessage.Progress.Target = retrievedMessageCounts.Sum();
                }
            }

            // This could be parallelized so that as soon as a channel's retrieval is completed, the deletion process begins for that channel
            // It would require a sophisticated system to correctly keep this in order, but it's not worth the effort right now
            await messageRetrievalTasks.WaitAll();
            
            completedBoxed.Value = true;
            await discoveryUpdate;

            var foundMessages = messageRetrievalTasks.Select(t => t.Result).Flatten().ToList();

            await DeleteFoundMessagesDifferentChannels(foundMessages, persistentProgressMessage);
        }

        private static bool IsSourceMessageDeleted(IMessage message)
        {
            return message.Flags.GetValueOrDefault().HasFlag(MessageFlags.SourceMessageDeleted);
        }
        #endregion

        #region Delete This Channel Deleted Announcement Messages
        [Command("delete da here")]
        [Alias("remove da here", "clear da here")]
        [Summary("Deletes all system-sent announcement messages that were deleted from the original source from the channel the command was sent in. Only deletes messages in that same channel.")]
        [Remarks(daCommandRemark)]
        public async Task DeleteThisChannelDeletedAnnouncementMessages()
        {
            await DeleteOtherChannelDeletedAnnouncementMessages(Context.Channel, 0, LargeSnowflake);
        }
        [Command("delete da here")]
        [Alias("remove da here", "clear da here")]
        [Summary("Deletes all system-sent announcement messages that were deleted from the original source from the channel the command was sent in that were sent after the provided message, **including** the first message. Only deletes messages in that same channel.")]
        [Remarks(daCommandRemark)]
        public async Task DeleteThisChannelDeletedAnnouncementMessages
        (
            [Summary("The ID of the first announcement message that will be deleted, **inclusive**.")]
            ulong firstMessageID
        )
        {
            await DeleteOtherChannelDeletedAnnouncementMessages(Context.Channel, firstMessageID, LargeSnowflake);
        }
        [Command("delete da here")]
        [Alias("remove da here", "clear da here")]
        [Summary("Deletes all system-sent announcement messages that were deleted from the original source from the channel the command was sent in that were sent within the provided message range, **including** the first and the last messages. Only deletes messages in that same channel.")]
        [Remarks(daCommandRemark)]
        public async Task DeleteThisChannelDeletedAnnouncementMessages
        (
            [Summary("The ID of the first announcement message that will be deleted, **inclusive**.")]
            ulong firstMessageID,
            [Summary("The ID of the last announcement message that will be deleted, **inclusive**. Must be greater than 0 and not less than the first message ID.")]
            ulong lastMessageID
        )
        {
            await DeleteOtherChannelDeletedAnnouncementMessages(Context.Channel, firstMessageID, lastMessageID);
        }
        #endregion

        #region Delete Other Channel Deleted Announcement Messages
        [Command("delete da oc")]
        [Alias("remove da oc", "clear da oc")]
        [Summary("Deletes all system-sent announcement messages that were deleted from the original source from a specified channel. Only deletes messages in that same channel.")]
        [Remarks(daCommandRemark)]
        public async Task DeleteOtherChannelDeletedAnnouncementMessages
        (
            [Summary("The channel on which the the messages to delete are contained.")]
            IMessageChannel textChannel
        )
        {
            await DeleteOtherChannelDeletedAnnouncementMessages(textChannel, 0, LargeSnowflake);
        }
        [Command("delete da oc")]
        [Alias("remove da oc", "clear da oc")]
        [Summary("Deletes all system-sent announcement messages that were deleted from the original source from a specified channel that were sent after the provided message, **including** the first message. Only deletes messages in that same channel.")]
        [Remarks(daCommandRemark)]
        public async Task DeleteOtherChannelDeletedAnnouncementMessages
        (
            [Summary("The channel on which the the messages to delete are contained.")]
            IMessageChannel textChannel,
            [Summary("The ID of the first announcement message that will be deleted, **inclusive**.")]
            ulong firstMessageID
        )
        {
            await DeleteOtherChannelDeletedAnnouncementMessages(textChannel, firstMessageID, LargeSnowflake);
        }
        [Command("delete da oc")]
        [Alias("remove da oc", "clear da oc")]
        [Summary("Deletes all system-sent announcement messages that were deleted from the original source from a specified channel that were sent within the provided message range, **including** the first and the last messages. Only deletes messages in that same channel.")]
        [Remarks(daCommandRemark)]
        public async Task DeleteOtherChannelDeletedAnnouncementMessages
        (
            [Summary("The channel on which the the messages to delete are contained.")]
            IMessageChannel textChannel,
            [Summary("The ID of the first announcement message that will be deleted, **inclusive**.")]
            ulong firstMessageID,
            [Summary("The ID of the last announcement message that will be deleted, **inclusive**. Must be greater than 0 and not less than the first message ID.")]
            ulong lastMessageID
        )
        {
            var contextChannel = Context.Channel;
            
            bool valid = await ValidateIDs(firstMessageID, lastMessageID);
            if (!valid)
                return;

            valid = await ValidateChannel(textChannel);
            if (!valid)
                return;

            var persistentProgressMessage = new MessageDeletingProgressPersistentMessage(contextChannel);
            var foundMessages = await DiscoverMessagesAsync(DiscoverFilteredMessagesAsync, persistentProgressMessage);

            await DeleteFoundMessages(foundMessages, persistentProgressMessage);

            async Task<HashSet<IMessage>> DiscoverFilteredMessagesAsync()
            {
                return await textChannel.GetMessageRangeAsync(firstMessageID, lastMessageID, earliestAnnouncementDate,
                                                              IsSourceMessageDeleted, null);
            }
        }
        #endregion
        #endregion

        private static async Task<HashSet<IMessage>> DiscoverMessagesAsync(Func<Task<HashSet<IMessage>>> messageDiscoverer, MessageDeletingProgressPersistentMessage persistentProgressMessage)
        {
            var completeBoxed = new BoxedStruct<bool>();
            var updateTask = persistentProgressMessage.KeepUpdatingDiscoveryMessage(750, completeBoxed);
            var foundMessages = await messageDiscoverer();

            completeBoxed.Value = true;
            await updateTask;

            return foundMessages;
        }

        private async Task<bool> ValidateChannel(IMessageChannel textChannel)
        {
            if (textChannel is not IGuildChannel guildChannel || guildChannel.GuildId != Context.Guild.Id)
            {
                await Context.Channel.SendMessageAsync("This server does not contain the provided channel.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that the provided IDs are valid and in the correct order.
        /// </summary>
        /// <param name="channel">The channel to report erroneous input in.</param>
        /// <param name="firstMessageID">The first message ID.</param>
        /// <param name="lastMessageID">The last message ID.</param>
        /// <returns><see langword="true"/> if the message IDs are valid, <see langword="false"/> otherwise.</returns>
        private async Task<bool> ValidateIDs(ulong firstMessageID, ulong lastMessageID)
        {
            if (lastMessageID == 0)
            {
                await Context.Channel.SendMessageAsync("The last message ID cannot be 0.");
                return false;
            }

            if (firstMessageID > lastMessageID)
            {
                await Context.Channel.SendMessageAsync("The first message ID cannot be greater than the last message ID.");
                return false;
            }

            return true;
        }

        private static async Task DeleteFoundMessagesDifferentChannels(IReadOnlyCollection<IMessage> foundMessages, MessageDeletingProgressPersistentMessage persistentProgressMessage)
        {
            if (foundMessages.Count is 0)
            {
                await persistentProgressMessage.ReportFinalizedProgress(5000);
                return;
            }
            
            var messagesByChannel = foundMessages.GroupBy(message => message.Channel).ToDictionary(grouping => grouping.Key, grouping => grouping.ToArray());
            var deleteTasks = new List<Task>();
            foreach (var channelMessages in messagesByChannel)
                deleteTasks.Add(DeleteFoundMessages(channelMessages.Value, persistentProgressMessage));

            await deleteTasks.WaitAll();
        }

        private static async Task DeleteFoundMessages(IReadOnlyCollection<IMessage> foundMessages, MessageDeletingProgressPersistentMessage persistentProgressMessage)
        {
            if (foundMessages.Count is 0)
            {
                await persistentProgressMessage.ReportFinalizedProgress(5000);
                return;
            }

            await persistentProgressMessage.UpdateActionProgress();
            
            var progressMessageTimestamp = persistentProgressMessage.CurrentMessage.Timestamp.UtcDateTime;
            
            var progressUpdateTask = persistentProgressMessage.KeepUpdatingProgressMessage(750, true, 5000);
            var messageChannel = foundMessages.First().Channel;
            await messageChannel.DeleteFoundMessages(foundMessages, progressMessageTimestamp, persistentProgressMessage.Progress);
            
            await progressUpdateTask;

            return;
        }
    }
}

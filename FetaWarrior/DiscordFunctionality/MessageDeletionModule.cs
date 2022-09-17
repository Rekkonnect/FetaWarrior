using Discord;
using Discord.Interactions;
using FetaWarrior.DiscordFunctionality.Interactions.Attributes;
using FetaWarrior.Extensions;
using FetaWarrior.Utilities;
using Garyon.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

[Group("delete", "Delete messages based on specified conditions")]
[RequireGuildContext]
[RequireUserPermission(ChannelPermission.ManageMessages, Group = "User")]
[RequireUserPermission(GuildPermission.ManageMessages, Group = "User")]
[RequireBotPermission(ChannelPermission.ManageMessages, Group = "Bot")]
[RequireBotPermission(GuildPermission.ManageMessages, Group = "Bot")]
public class MessageDeletionModule : SocketInteractionModule
{
    [SlashCommand("range", "Delete a range of messages in a channel")]
    public async Task DeleteRange
    (
        [Summary(description: "The channel whose messages to delete, defaulting to this channel")]
        ITextChannel textChannel = null,
        [Summary(description: "The ID of the first message that will be deleted, inclusive")]
        Snowflake firstMessageID = default,
        [Summary(description: "The ID of the last message that will be deleted, inclusive")]
        Snowflake lastMessageID = default
    )
    {
        var contextChannel = Context.Channel;
        textChannel ??= contextChannel as ITextChannel;

        if (lastMessageID == 0)
            lastMessageID = Snowflake.LargeSnowflake;

        bool valid = await ValidateMessageIDs(firstMessageID, lastMessageID);
        if (!valid)
            return;

        valid = await ValidateChannel(textChannel);
        if (!valid)
            return;

        var progressMessageTimestamp = Context.Interaction.CreatedAt;        
        await RespondAsync("Discovering the messages to delete.");
        var persistentProgressMessage = new MessageDeletingProgressPersistentMessage(Context.Interaction);

        lastMessageID = Math.Min(lastMessageID, persistentProgressMessage.CurrentMessage.Id - 1);

        var foundMessages = await DiscoverMessagesAsync(DiscoverFilteredMessagesAsync, persistentProgressMessage);

        async Task<HashSet<IMessage>> DiscoverFilteredMessagesAsync()
        {
            return await textChannel.GetMessageRangeAsync(firstMessageID, lastMessageID, persistentProgressMessage.Progress);
        }

        var updateTask = persistentProgressMessage.KeepUpdatingProgressMessage(750, true);
        
        await textChannel.DeleteFoundMessages(foundMessages, progressMessageTimestamp.UtcDateTime, persistentProgressMessage.Progress);

        await updateTask;
    }

    [Group("removed-an", "Delete all received announcement messages that were deleted from the original source")]
    public class RemovedAnnouncements : SocketInteractionModule
    {
        //                              DD/MM/YYYY
        // The article was published in 11/11/2020, but we're adding an estimated extra month for beta rollout
        // In the event that we get followed announcement messages prior to that date, the users should file an issue to fix this
        private static readonly DateTime earliestAnnouncementDate = new(2020, 10, 10);
        private const string earliestAnnouncementDateString = "10/10/2020";
        private const string daCommandRemarkStatement0 = $@"The messages are filtered to dates not prior to {earliestAnnouncementDateString}, which the current estimate of the earliest announcement messages.";
        private const string daCommandRemarkStatement1 = $@"Please report messages uncaught by this filter.";
        private const string daCommandRemarkStatement2 = $@"Search for ""[Original Message Deleted]"" after every operation to guarantee no leftovers.";
        private const string daCommandRemark = $@"{daCommandRemarkStatement0} {daCommandRemarkStatement1} {daCommandRemarkStatement2}";

        // TODO: Build indexing (database moment)
        [SlashCommand("all-channels", "Delete all received announcement messages that were deleted from the original source")]
        public async Task DeleteServerWideDeletedAnnouncementMessages
        (
            [Summary(description: "The ID of the first announcement message that will be deleted, inclusive")]
            Snowflake firstMessageID = default,
            [Summary(description: "The ID of the last announcement message that will be deleted, inclusive")]
            Snowflake lastMessageID = default
        )
        {
            if (lastMessageID == 0)
                lastMessageID = Snowflake.LargeSnowflake;

            bool valid = await ValidateMessageIDs(firstMessageID, lastMessageID);
            if (!valid)
                return;

            var persistentProgressMessage = new MessageDeletingProgressPersistentMessage(Context.Interaction);

            // TODO: Abstract this logic away to another component
            var textChannels = Context.Guild.TextChannels;
            var messageRetrievalTasks = new Task<HashSet<IMessage>>[textChannels.Count];
            var channelProgresses = new Progress[textChannels.Count];
            for (int i = 0; i < textChannels.Count; i++)
                channelProgresses[i] = new();

            var completedBoxed = new BoxedStruct<bool>();
            var discoveryUpdate = persistentProgressMessage.KeepUpdatingDiscoveryMessage(750, completedBoxed);

            foreach (var (index, textChannel) in textChannels.WithIndex())
            {
                var channelProgress = channelProgresses[index];

                channelProgress.Updated += ChannelProgressUpdated;

                var task = textChannel.GetMessageRangeAsync(firstMessageID, lastMessageID, earliestAnnouncementDate,
                    IsSourceMessageDeleted, channelProgress);
                messageRetrievalTasks[index] = task;

                void ChannelProgressUpdated()
                {
                    // This could be a system but it's fine
                    persistentProgressMessage.Progress.Target = channelProgresses.Sum(p => p.Target);
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

        private void ChannelProgressUpdated()
        {
            throw new NotImplementedException();
        }

        private static bool IsSourceMessageDeleted(IMessage message)
        {
            return message.Flags.GetValueOrDefault().HasFlag(MessageFlags.SourceMessageDeleted);
        }

        [SlashCommand("range", "Delete all received announcement messages that were deleted from the original source")]
        public async Task DeleteDeletedAnnouncementRange
        (
            [Summary(description: "The channel on which the the messages to delete are contained")]
            ITextChannel textChannel = null,
            [Summary(description: "The ID of the first announcement message that will be deleted, inclusive")]
            Snowflake firstMessageID = default,
            [Summary(description: "The ID of the last announcement message that will be deleted, inclusive")]
            Snowflake lastMessageID = default
        )
        {
            var contextChannel = Context.Channel;
            textChannel ??= contextChannel as ITextChannel;

            if (lastMessageID == 0)
                lastMessageID = Snowflake.LargeSnowflake;

            bool valid = await ValidateMessageIDs(firstMessageID, lastMessageID);
            if (!valid)
                return;

            valid = await ValidateChannel(textChannel);
            if (!valid)
                return;

            await RespondAsync("Discovering deleted announcement messages to remove.");

            var persistentProgressMessage = new MessageDeletingProgressPersistentMessage(Context.Interaction);
            var foundMessages = await DiscoverMessagesAsync(DiscoverFilteredMessagesAsync, persistentProgressMessage);

            await DeleteFoundMessages(foundMessages, persistentProgressMessage);

            async Task<HashSet<IMessage>> DiscoverFilteredMessagesAsync()
            {
                return await textChannel.GetMessageRangeAsync(firstMessageID, lastMessageID, earliestAnnouncementDate,
                                                              IsSourceMessageDeleted, persistentProgressMessage.Progress);
            }
        }
    }

    private static async Task<HashSet<IMessage>> DiscoverMessagesAsync(Func<Task<HashSet<IMessage>>> messageDiscoverer, MessageDeletingProgressPersistentMessage persistentProgressMessage)
    {
        var completeBoxed = new BoxedStruct<bool>();
        var updateTask = persistentProgressMessage.KeepUpdatingDiscoveryMessage(750, completeBoxed);
        var foundMessages = await messageDiscoverer();

        completeBoxed.Value = true;
        await updateTask;

        return foundMessages;
    }

    private static async Task DeleteFoundMessagesDifferentChannels(IReadOnlyCollection<IMessage> foundMessages, MessageDeletingProgressPersistentMessage persistentProgressMessage)
    {
        if (foundMessages.Count is 0)
            return;
        
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
            await persistentProgressMessage.SetContentAsync("No messages were found to delete.");
            return;
        }

        await persistentProgressMessage.UpdateActionProgress();
        
        var progressMessageTimestamp = persistentProgressMessage.CurrentMessage.Timestamp.UtcDateTime;
        
        var progressUpdateTask = persistentProgressMessage.KeepUpdatingProgressMessage(750, true);
        var messageChannel = foundMessages.First().Channel;
        await messageChannel.DeleteFoundMessages(foundMessages, progressMessageTimestamp, persistentProgressMessage.Progress);
        
        await progressUpdateTask;
    }
}

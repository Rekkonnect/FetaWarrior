using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FetaWarrior.Extensions
{
    public static class IMessageChannelExtensions
    {
        public static async Task<IMessage> GetFirstMessageAsync(this IMessageChannel channel)
        {
            return (await channel.GetMessagesAsync(0, Direction.After, 1).FlattenAsync()).FirstOrDefault();
        }
        public static async Task<IMessage> GetLastMessageAsync(this IMessageChannel channel)
        {
            return (await channel.GetMessagesAsync(1).FlattenAsync()).FirstOrDefault();
        }

        public static async Task<HashSet<IMessage>> GetMessageRangeAsync(this IMessageChannel channel, ulong firstMessageID, ulong lastMessageID)
        {
            return await GetMessageRangeAsync(channel, firstMessageID, lastMessageID, null, null);
        }
        public static async Task<HashSet<IMessage>> GetMessageRangeAsync(this IMessageChannel channel, ulong firstMessageID, ulong lastMessageID, Predicate<IMessage> messagePredicate)
        {
            return await GetMessageRangeAsync(channel, firstMessageID, lastMessageID, messagePredicate, null);
        }
        public static async Task<HashSet<IMessage>> GetMessageRangeAsync(this IMessageChannel channel, ulong firstMessageID, ulong lastMessageID, Func<int, Task> progressReporter)
        {
            return await GetMessageRangeAsync(channel, firstMessageID, lastMessageID, null, progressReporter);
        }
        public static async Task<HashSet<IMessage>> GetMessageRangeAsync(this IMessageChannel channel, ulong firstMessageID, ulong lastMessageID, Predicate<IMessage> messagePredicate, Func<int, Task> progressReporter)
        {
            var result = new HashSet<IMessage>();

            // firstMessageID - 1 because the message retriever method retrieves messages after the given message's ID, excluding the original one
            for (ulong currentID = firstMessageID - 1; currentID < lastMessageID;)
            {
                var messages = await channel.GetMessagesAsync(currentID, Direction.After, DiscordConfig.MaxMessagesPerBatch).FlattenAsync();

                foreach (var message in messages)
                {
                    var id = message.Id;

                    // Set the current ID to the maximum found ID
                    if (id > currentID)
                        currentID = id;

                    if (id > lastMessageID)
                        continue;

                    if (messagePredicate?.Invoke(message) == false)
                        continue;

                    result.Add(message);
                }

                if (progressReporter != null)
                    await progressReporter.Invoke(result.Count);
            }

            return result;
        }
    }
}

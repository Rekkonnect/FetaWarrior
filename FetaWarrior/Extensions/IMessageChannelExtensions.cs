using Discord;
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
    }
}

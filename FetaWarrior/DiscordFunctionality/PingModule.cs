using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality
{
    public class PingModule : ModuleBase<TimestampedSocketCommandContext>
    {
        [Command("ping")]
        [Summary("Gets the current ping.")]
        public async Task PingAsync()
        {
            var beforeSending = DateTime.Now;

            var message = await ReplyAsync("Calculating ping...");

            var afterSending = DateTime.Now;
            var sendingLatency = afterSending - beforeSending;

            var totalPing = sendingLatency + Context.RetrievalLatency;
            await message.ModifyAsync(m => m.Content =
$@"Current Ping: `{totalPing.TotalMilliseconds:N0}ms`

Details:
Retrieval Latency: `{Context.RetrievalLatency.TotalMilliseconds:N0}ms`
Sending Latency: `{sendingLatency.TotalMilliseconds:N0}ms`");
        }
    }
}

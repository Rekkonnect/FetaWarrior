using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FetaWarrior.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

[EnabledInDm(false)]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageChannels)]
[RequireBotPermission(GuildPermission.ManageChannels)]
public class ChannelCategoryModule : SocketInteractionModule
{
    [EnabledInDm(false)]
    [SlashCommand("category", "Adjust channels' parent categories")]
    public async Task MoveChannels
    (
        [Summary(description: "The channel category whose channels to move (leave empty for uncategorized channels)")]
        ICategoryChannel originalCategory = null,
        [Summary(description: "The new channel category to move the channels into (leave empty to make them uncategorized)")]
        ICategoryChannel targetCategory = null,

        [Summary(description: "Set this to true to delete the original category (if any) after moving all the channels")]
        bool deleteOriginalCategory = false
    )
    {
        ulong originalCategoryGuildID = originalCategory?.GuildId ?? Context.Guild.Id;
        ulong targetCategoryGuildID = targetCategory?.GuildId ?? Context.Guild.Id;

        if (originalCategoryGuildID != Context.Guild.Id ||
            targetCategoryGuildID != Context.Guild.Id)
        {
            await RespondAsync("Both channel categories must be in this server that the interaction was created.");
            return;
        }

        if (originalCategory?.Id == targetCategory?.Id)
        {
            await RespondAsync("The channel categories must differ and there must be at least one specified category.");
            return;
        }

        await RespondAsync("Discovering channels to move...");

        var originalCategorySocket = originalCategory as SocketCategoryChannel;
        var targetCategorySocket = targetCategory as SocketCategoryChannel;

        var tree = new GuildChannelTree(Context.Guild);
        var movedChannels = tree.MoveIntoCategory(originalCategorySocket, targetCategorySocket);
        var reorders = tree.GetReoderingInformation().ToList();
        if (reorders.Count > 0)
        {
            await UpdateResponseTextAsync("Reordering the channels to fit in the categories...");
            await Context.Guild.ReorderChannelsAsync(reorders, new() { AuditLogReason = "Moving channels into category" });
        }

        foreach (var channel in movedChannels)
        {
            await channel.MoveToCategoryAsync(targetCategory);
        }

        if (deleteOriginalCategory && originalCategorySocket is not null)
        {
            await UpdateResponseTextAsync("Deleting the category channel...");

            await originalCategorySocket.DeleteAsync();
        }

        await UpdateResponseTextAsync("Successfully moved the channels");
    }
}
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FetaWarrior.Extensions;
using Garyon.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

[Group("category", "Adjust multiple channels inside categories")]
[EnabledInDm(false)]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageChannels)]
[RequireBotPermission(GuildPermission.ManageChannels)]
public class ChannelCategoryModule : SocketInteractionModule
{
    [EnabledInDm(false)]
    [SlashCommand("move", "Adjust channels' parent categories")]
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

        if (deleteOriginalCategory && originalCategory is not null)
        {
            await UpdateResponseTextAsync("Deleting the category channel...");

            await originalCategory.DeleteAsync();
        }

        await UpdateResponseTextAsync("Successfully moved the channels.");
    }

    [EnabledInDm(false)]
    [SlashCommand("delete", "Delete all channels belonging to a category")]
    public async Task DeleteChannels
    (
        [Summary(description: "The channel category whose channels to delete (leave empty for uncategorized channels)")]
        ICategoryChannel parentCategory = null,

        [Summary(description: "Set this to true to delete the original category (if any) after deleting all the channels")]
        bool deleteCategory = false
    )
    {
        var filters = await ShowChannelFilterMenu();

        var targetChannels = Context.Guild.ChannelsInCategory(parentCategory?.Id);
        var filteredChannels = Filter(targetChannels, filters).ToArray();

        foreach (var channel in filteredChannels)
        {
            await channel.DeleteAsync();
        }

        if (deleteCategory && parentCategory is not null)
        {
            await UpdateResponseTextAsync("Deleting the category channel...");

            await parentCategory.DeleteAsync();
        }

        await UpdateResponseTextAsync("Successfully deleted the channels of the specified types.");
    }

    private static IEnumerable<IGuildChannel> Filter(IEnumerable<IGuildChannel> channels, ChannelTypeFilterArguments arguments)
    {
        return channels.Where(c => arguments.PassesFilter(c));
    }

    private static readonly NestedChannelType[] nestedChannelTypes =
    {
        NestedChannelType.Text,
        NestedChannelType.Voice,
        NestedChannelType.Stage,
        NestedChannelType.Forum,
        NestedChannelType.Announcement,
    };

    protected async Task<ChannelTypeFilterArguments> ShowChannelFilterMenu()
    {
        var deferral = DeferAsync();

        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Select the channel types")
            .WithCustomId("channel_type_menu")
            .WithMinValues(1)
            .WithMaxValues(nestedChannelTypes.Length);

        foreach (var channelType in nestedChannelTypes)
        {
            var channelTypeName = channelType.ToString();
            menuBuilder.AddOption(channelTypeName, channelTypeName);
        }

        var componentBuilder = new ComponentBuilder()
            .WithSelectMenu(menuBuilder);

        const string forumChannelWarning = "**NOTE: Forum channels cannot be discovered in channel categories for the time being. The option will be ignored.**";

        await deferral;
        var selectionMessage = await FollowupAsync(text: $"Select the channel types to delete:\n{forumChannelWarning}", components: componentBuilder.Build(), ephemeral: true);

        var menuResponse = await TrackSelectMenuResponseAsync(selectionMessage.Id, Context.Interaction.User, null);
        var values = menuResponse.Data.Values;

        var delimitedValues = string.Join(", ", values);
        await selectionMessage.ModifyAsync(m =>
        {
            m.Content = $"Deleting channel types: **{delimitedValues}**";
            m.Components = new Optional<MessageComponent>(null);
        });

        var types = TypesForStringValues(values);
        return ChannelTypeFilterArguments.FromTypes(types);
    }

    private IEnumerable<NestedChannelType> TypesForStringValues(IEnumerable<string> values)
    {
        return values.Select(TypesForStringValue);
    }
    private NestedChannelType TypesForStringValue(string value)
    {
        return value switch
        {
            nameof(NestedChannelType.Text) => NestedChannelType.Text,
            nameof(NestedChannelType.Voice) => NestedChannelType.Voice,
            nameof(NestedChannelType.Stage) => NestedChannelType.Stage,
            nameof(NestedChannelType.Announcement) => NestedChannelType.Announcement,
            nameof(NestedChannelType.Forum) => NestedChannelType.Forum,
        };
    }

    private async Task<SocketMessageComponent> TrackSelectMenuResponseAsync(ulong messageID, IUser guildUser, Func<SocketMessageComponent, Task> modalSubmitted)
    {
        BotClientManager.Instance.Client.SelectMenuExecuted += FireOnSpecificToken;

        SocketMessageComponent targetMenu = null;
        while (targetMenu is null)
        {
            await Task.Delay(50);
        }

        BotClientManager.Instance.Client.SelectMenuExecuted -= FireOnSpecificToken;
        return targetMenu;

        async Task FireOnSpecificToken(SocketMessageComponent messageComponent)
        {
            if (messageComponent.Message.Id != messageID)
                return;

            if (messageComponent.User.Id != guildUser.Id)
                return;

            // Allow removing the event as quickly as possible
            targetMenu = messageComponent;

            if (modalSubmitted is not null)
            {
                await modalSubmitted(messageComponent);
            }
        }
    }

    private async Task<SocketModal> TrackModalResponseAsync(string interactionToken, Func<SocketModal, Task>? modalSubmitted)
    {
        BotClientManager.Instance.Client.ModalSubmitted += FireOnSpecificToken;

        SocketModal targetModal = null;
        while (targetModal is null)
        {
            await Task.Delay(50);
        }

        BotClientManager.Instance.Client.ModalSubmitted -= FireOnSpecificToken;
        return targetModal;

        async Task FireOnSpecificToken(SocketModal modal)
        {
            if (modal.Token != interactionToken)
                return;

            // Allow removing the event as quickly as possible
            targetModal = modal;

            if (modalSubmitted is not null)
            {
                await modalSubmitted(modal);
            }
        }
    }

    protected async Task<ChannelTypeFilterArguments> AskChannelTypeFilterArguments()
    {
        var filters = new ChannelTypeFilterArguments();

        var componentBuilder = new ComponentBuilder()

            .WithButton(label: nameof(ChannelTypeFilterArguments.Text),
                        customId: nameof(ChannelTypeFilterArguments.Text),
                        style: ToggledButtonStyle(filters.Text))

            .WithButton(label: nameof(ChannelTypeFilterArguments.Voice),
                        customId: nameof(ChannelTypeFilterArguments.Voice),
                        style: ToggledButtonStyle(filters.Voice))

            .WithButton(label: nameof(ChannelTypeFilterArguments.Stage),
                        customId: nameof(ChannelTypeFilterArguments.Stage),
                        style: ToggledButtonStyle(filters.Stage))

            .WithButton(label: nameof(ChannelTypeFilterArguments.Announcement),
                        customId: nameof(ChannelTypeFilterArguments.Announcement),
                        style: ToggledButtonStyle(filters.Announcement))

            .WithButton(label: nameof(ChannelTypeFilterArguments.Forum),
                        customId: nameof(ChannelTypeFilterArguments.Forum),
                        style: ToggledButtonStyle(filters.Forum));


        await RespondAsync(
            ephemeral: true,
            components: componentBuilder.Build());

        return filters;
    }
    private static ButtonStyle ToggledButtonStyle(bool toggled)
    {
        return toggled switch
        {
            true => ButtonStyle.Success,
            false => ButtonStyle.Danger,
        };
    }
}

using Discord;
using Discord.WebSocket;
using FetaWarrior.Extensions;
using Garyon.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FetaWarrior.DiscordFunctionality;

#nullable enable

public sealed class GuildChannelTree
{
    private readonly GuildCategoryNodes nodes;

    public SocketGuild Guild => nodes.Guild;

    public GuildChannelTree(SocketGuild guild)
    {
        nodes = new(guild);
    }

    public IEnumerable<ReorderChannelProperties> GetReoderingInformation()
    {
        return GetNestedReorderingInformation().Concat(GetCategoryReorderingInformation());
    }
    private IEnumerable<ReorderChannelProperties> GetNestedReorderingInformation()
    {
        var reorders = new List<ReorderChannelProperties>();

        int textIndex = 0;
        int voiceIndex = 0;
        foreach (var node in nodes)
        {
            IterateChannelCollection(reorders, node.TextChannels, ref textIndex);
            IterateChannelCollection(reorders, node.VoiceChannels, ref voiceIndex);
        }

        return reorders;
    }
    private static void IterateChannelCollection(List<ReorderChannelProperties> reorders, IEnumerable<IGuildChannel> channels, ref int index)
    {
        foreach (var channel in channels)
        {
            if (channel.Position != index)
            {
                reorders.Add(channel.ReorderInto(index));
            }
            index++;
        }
    }

    private IEnumerable<ReorderChannelProperties> GetCategoryReorderingInformation()
    {
        // For the time being, category channels are not being reordered
        return Enumerable.Empty<ReorderChannelProperties>();
    }

    public IEnumerable<INestedChannel> MoveIntoCategory(SocketCategoryChannel? source, SocketCategoryChannel? target)
    {
        var sourceNode = nodes[source];
        var targetNode = nodes[target];

        return sourceNode.MergeInto(targetNode);
    }

    private sealed class GuildCategoryNodes : IEnumerable<CategoryNode>
    {
        public SocketGuild Guild { get; }

        private CategoryNode voidCategoryNode;
        private List<CategoryNode> categoryNodes;
        private Dictionary<SocketCategoryChannel, CategoryNode> categoryNodeMap = new();

        public GuildCategoryNodes(SocketGuild guild)
        {
            Guild = guild;
            InitializeTree();
        }

        public int TextChannelStartingPosition(int categoryPosition)
        {
            return ChannelStartingPosition(categoryPosition, ChannelType.Text);
        }
        public int VoiceChannelStartingPosition(int categoryPosition)
        {
            return ChannelStartingPosition(categoryPosition, ChannelType.Voice);
        }
        public int ChannelStartingPosition(int categoryPosition, ChannelType type)
        {
            while (categoryPosition >= -1)
            {
                var category = this[categoryPosition];
                var channel = category.ListForChannelType(type).FirstOrDefault();
                if (channel is not null)
                    return channel.Position + 1;

                categoryPosition--;
            }
            return 0;
        }

        public int TextChannelAppendingPosition(int categoryPosition)
        {
            return ChannelAppendingPosition(categoryPosition, ChannelType.Text);
        }
        public int VoiceChannelAppendingPosition(int categoryPosition)
        {
            return ChannelAppendingPosition(categoryPosition, ChannelType.Voice);
        }
        public int ChannelAppendingPosition(int categoryPosition, ChannelType type)
        {
            var lastChannel = this[categoryPosition].ListForChannelType(type).LastOrDefault();
            if (lastChannel is not null)
                return lastChannel.Position + 1;

            while (categoryPosition < categoryNodes.Count - 1)
            {
                categoryPosition++;
                var category = this[categoryPosition];
                var channel = category.ListForChannelType(type).FirstOrDefault();
                if (channel is not null)
                    return channel.Position;
            }
            return 0;
        }

        private void InitializeTree()
        {
            voidCategoryNode = CategoryNode.CreateForVoidCategory(Guild);
            categoryNodes = new()
            {
                voidCategoryNode,
            };

            var nonVoidCategoryNodes = Guild.CategoryChannels.OrderBy(c => c.Position).Select(c => new CategoryNode(c));
            foreach (var node in nonVoidCategoryNodes)
            {
                categoryNodes.Add(node);
                categoryNodeMap[node.CategoryChannel] = node;
            }
        }

        public IEnumerator<CategoryNode> GetEnumerator() => categoryNodes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public CategoryNode this[int index]
        {
            get
            {
                // -1 maps to the void category, which is at index 0
                return categoryNodes[index + 1];
            }
        }
        public CategoryNode this[SocketCategoryChannel? category]
        {
            get
            {
                if (category is null)
                    return voidCategoryNode;

                return categoryNodeMap[category];
            }
        }
    }

    private sealed class CategoryNode
    {
        public SocketCategoryChannel? CategoryChannel { get; }

        // So I found this bug about IForumChannel not being an INestedChannel
        public List<IGuildChannel> TextChannels { get; private set; } = new();
        public List<IVoiceChannel> VoiceChannels { get; private set; } = new();

        public int CategoryPosition => CategoryChannel?.Position ?? -1;

        private CategoryNode() { }

        public CategoryNode(SocketCategoryChannel channel)
        {
            CategoryChannel = channel;
            Initialize();
        }

        public IEnumerable<IGuildChannel> ListForChannelType(ChannelType channelType)
        {
            return channelType switch
            {
                ChannelType.Text or
                ChannelType.Forum or
                ChannelType.News => TextChannels,

                ChannelType.Voice or
                ChannelType.Stage => VoiceChannels,
            };
        }

        public void UpdatePositions()
        {
            TextChannels = TextChannels.OrderBy(c => c.Position).ToList();
            VoiceChannels = VoiceChannels.OrderBy(c => c.Position).ToList();
        }

        public IEnumerable<INestedChannel> MergeInto(CategoryNode target)
        {
            // Assume correct positioning of the nested channels
            if (CategoryPosition > target.CategoryPosition)
            {
                target.TextChannels.AddRange(TextChannels);
                target.VoiceChannels.AddRange(VoiceChannels);
            }
            else
            {
                target.TextChannels.InsertRange(0, TextChannels);
                target.VoiceChannels.InsertRange(0, VoiceChannels);
            }

            var movedChannels = GetOrderedChannels().ToList();

            TextChannels.Clear();
            VoiceChannels.Clear();

            return movedChannels;
        }

        public IEnumerable<INestedChannel> GetOrderedChannels()
        {
            return TextChannels.Cast<INestedChannel>().Concat(VoiceChannels);
        }

        public void AppendChannel(IGuildChannel channel)
        {
            switch (channel)
            {
                // Voice channels are basically better text channels
                // This SocketVoiceChannel implements both interfaces, so we rely on order of comparison
                case IVoiceChannel voice:
                    VoiceChannels.Add(voice);
                    break;

                // Add leaving pattern variables for until the abstraction is fixed
                case ITextChannel text:
                case IForumChannel forum:
                    TextChannels.Add(channel);
                    break;
            }
        } 

        private void Initialize()
        {
            Initialize(CategoryChannel!.Channels);
        }
        private void Initialize(IEnumerable<IGuildChannel> channels)
        {
            foreach (var channel in channels.OrderBy(c => c.Position))
            {
                AppendChannel(channel);
            }
        }

        public static CategoryNode CreateForVoidCategory(SocketGuild guild)
        {
            var uncategorized = guild.UncategorizedChannels();
            var node = new CategoryNode();
            node.Initialize(uncategorized);
            return node;
        }
    }
}

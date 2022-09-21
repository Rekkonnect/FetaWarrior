using Discord;
using System.Threading.Tasks;

namespace FetaWarrior.Extensions;

public static class INestedChannelExtensions
{
    public static async Task MoveToCategoryAsync(this INestedChannel channel, ICategoryChannel category)
    {
        await channel.ModifyAsync(c => c.CategoryId = category?.Id);
    }
}

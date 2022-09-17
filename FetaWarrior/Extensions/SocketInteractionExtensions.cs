using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace FetaWarrior.Extensions;

public static class IDiscordInteractionExtensions
{
    public static async Task UpdateResponseTextAsync(this IDiscordInteraction interaction, string text)
    {
        await interaction.ModifyOriginalResponseAsync(m => m.Content = text);
    }
}

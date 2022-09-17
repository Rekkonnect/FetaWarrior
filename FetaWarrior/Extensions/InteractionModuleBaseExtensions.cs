using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace FetaWarrior.Extensions;

public static class InteractionModuleBaseExtensions
{
    public static async Task UpdateResponseTextAsync<TContext>(this InteractionModuleBase<TContext> module, string text)
        where TContext : class, IInteractionContext
    {
        await module.UpdateResponseTextAsync(text);
    }
}

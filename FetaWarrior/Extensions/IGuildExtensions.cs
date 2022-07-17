using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FetaWarrior.Extensions;

public static class IGuildExtensions
{
    public static async Task<IReadOnlyCollection<IBan>> GetAllBansAsync(this IGuild guild)
    {
        return (await guild.GetBansAsync().FlattenAsync()).ToArray();
    }
}

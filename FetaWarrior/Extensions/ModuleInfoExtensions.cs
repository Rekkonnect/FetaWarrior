using Discord.Commands;
using System.Linq;

namespace FetaWarrior.Extensions;

public static class ModuleInfoExtensions
{
    public static bool HasPrecondition<T>(this ModuleInfo info)
        where T : PreconditionAttribute
    {
        return info.Preconditions.Any(a => a is T);
    }
}

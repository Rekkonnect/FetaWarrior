using Discord.Commands;
using System.Linq;

namespace FetaWarrior.Extensions;

public static class CommandInfoExtensions
{
    public static bool HasPrecondition<T>(this CommandInfo info)
        where T : PreconditionAttribute
    {
        return info.Preconditions.Any(a => a is T) || info.Module.HasPrecondition<T>();
    }
}

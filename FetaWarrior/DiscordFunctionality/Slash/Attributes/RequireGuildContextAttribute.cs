using Discord.Interactions;
using System;

namespace FetaWarrior.DiscordFunctionality.Slash.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class RequireGuildContextAttribute : RequireContextAttribute
{
    public RequireGuildContextAttribute()
        : base(ContextType.Guild) { }
}

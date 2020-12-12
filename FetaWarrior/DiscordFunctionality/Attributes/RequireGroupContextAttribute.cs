using Discord.Commands;
using System;

namespace FetaWarrior.DiscordFunctionality.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireGroupContextAttribute : RequireContextAttribute
    {
        public RequireGroupContextAttribute()
            : base(ContextType.Group) { }
    }
}

﻿using Discord.Commands;
using System;

namespace FetaWarrior.DiscordFunctionality.Slash.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class RequireDMContextAttribute : RequireContextAttribute
{
    public RequireDMContextAttribute()
        : base(ContextType.DM) { }
}
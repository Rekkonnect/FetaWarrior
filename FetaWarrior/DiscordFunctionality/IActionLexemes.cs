﻿namespace FetaWarrior.DiscordFunctionality;

public interface IActionLexemes
{
    public abstract string ActionName { get; }
    public abstract string ActionPastParticiple { get; }

    public abstract string ObjectName { get; }
    public abstract string ObjectNamePlural { get; }
}
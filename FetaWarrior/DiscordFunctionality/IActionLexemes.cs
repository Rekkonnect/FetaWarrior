namespace FetaWarrior.DiscordFunctionality;

// TODO: Use absrtact statics?
public interface IActionLexemes
{
    public abstract string ActionName { get; }
    public abstract string ActionPastParticiple { get; }

    public abstract string ObjectName { get; }
    public abstract string ObjectNamePlural { get; }
}
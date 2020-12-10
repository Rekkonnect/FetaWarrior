using Discord.Commands;

namespace FetaWarrior.DiscordFunctionality
{
    /// <summary>Provides a type reader that also indicates the object type that is being read.</summary>
    /// <typeparam name="T">The type of the object being read.</typeparam>
    public abstract class TypeReader<T> : TypeReader { }
}

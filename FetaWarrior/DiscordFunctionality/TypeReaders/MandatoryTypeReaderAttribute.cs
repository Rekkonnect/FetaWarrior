using System;

namespace FetaWarrior.DiscordFunctionality.TypeReaders
{
    /// <summary>Marks a <seealso cref="TypeReader"/> as a mandatory type reader to be initialized upon initializing the <seealso cref="CommandHandler"/>.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class MandatoryTypeReaderAttribute : Attribute
    {

    }
}

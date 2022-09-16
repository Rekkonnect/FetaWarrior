using Discord;

namespace FetaWarrior.DiscordFunctionality;

public class MassYeetingProgressPersistentMessage : ProgressPersistentMessage
{
    private readonly MassYeetUsersModuleBase module;

    public override IActionLexemes Lexemes => module.Lexemes;

    public MassYeetingProgressPersistentMessage(MassYeetUsersModuleBase massYeetModule, IUserMessage currentMessage)
        : base(currentMessage)
    {
        module = massYeetModule;
    }
}

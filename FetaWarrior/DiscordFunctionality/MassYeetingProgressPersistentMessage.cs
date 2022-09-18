namespace FetaWarrior.DiscordFunctionality;

public class MassYeetingProgressPersistentMessage : ProgressPersistentMessage
{
    private readonly MassYeetUsersModuleBase module;

    public override IActionLexemes Lexemes => module.Lexemes;

    public MassYeetingProgressPersistentMessage(MassYeetUsersModuleBase massYeetModule)
        : base(massYeetModule.Context.Interaction)
    {
        module = massYeetModule;
    }
}

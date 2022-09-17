using Discord;
using System;

namespace FetaWarrior.DiscordFunctionality;

public class MassYeetingProgressPersistentMessage : ProgressPersistentMessage
{
    private readonly MassYeetUsersModuleBase module;

    public override IActionLexemes Lexemes => module.Lexemes;

    [Obsolete]
    public MassYeetingProgressPersistentMessage(MassYeetUsersModuleBase massYeetModule, IUserMessage currentMessage)
        : base(currentMessage)
    {
        module = massYeetModule;
    }
    public MassYeetingProgressPersistentMessage(MassYeetUsersModuleBase massYeetModule)
        : base(massYeetModule.Context.Interaction)
    {
        module = massYeetModule;
    }
}

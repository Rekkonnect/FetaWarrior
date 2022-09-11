using Discord;
using Discord.Interactions;
using Garyon.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TypeEx = Garyon.Reflection.TypeExtensions;

namespace FetaWarrior.DiscordFunctionality.Slash;

// FOR VERY ADVANCED SLASH COMMANDS?
// This is not necessary, is it?
// TODO: Remove if proven useless; make sure to ignore the diff from git before pushing the first implementation
public abstract class SlashCommandContainer
{
    protected abstract SlashCommandBuilder CreateSlashCommandBuilder();
}

public class SlashCommandDiscoverer
{
    public static SlashCommandDiscoverer Instance { get; } = new();

    private SlashCommandDiscoverer() { }

    public IEnumerable<SlashCommandContainer> GetAllSlashCommandContainers()
    {
        var entry = Assembly.GetEntryAssembly();
        return entry.GetTypes()
            .Where(t => t.Inherits<SlashCommandContainer>())
            .Select(TypeEx.InitializeInstance<SlashCommandContainer>)
            .ToArray();
    }
    public IEnumerable<SlashCommandBuilder> GetAllBuilders()
    {
        return null;
    }
}

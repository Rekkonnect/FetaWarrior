using Discord;
using Discord.WebSocket;
using FetaWarrior.Utilities;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

public abstract class ProgressPersistentMessage : InitializablePersistentMessage
{
    public Progress Progress { get; } = new();

    public abstract IActionLexemes Lexemes { get; }

    protected ProgressPersistentMessage(IUserMessage currentMessage)
        : base(currentMessage) { }
    protected ProgressPersistentMessage(ISocketMessageChannel channel)
        : base(channel) { }
    protected ProgressPersistentMessage(IDiscordInteraction interaction)
        : base(interaction) { }

    protected override string GetInitializationMessageContent()
    {
        return GetDiscoveryProgressMessage();
    }

    protected string GetDiscoveryProgressMessage()
    {
        return $"Discovering {Lexemes.ObjectNamePlural} to {Lexemes.ActionName}... {Progress?.Target ?? 0} {Lexemes.ObjectNamePlural} have been found so far.";
    }
    protected string GetActionProgressMessage()
    {
        return $"{Progress.Current} of {Progress.Target} ({Progress.Ratio:P2}) {Lexemes.ObjectNamePlural} have been {Lexemes.ActionPastParticiple}...";
    }
    protected string GetFinalizationMessage()
    {
        return $"{Progress.Target} {Lexemes.ObjectNamePlural} have been {Lexemes.ActionPastParticiple}.";
    }

    public async Task UpdateDiscoveryProgress()
    {
        await SetContentAsync(GetDiscoveryProgressMessage());
    }
    public async Task UpdateDiscoveryProgress(int newTarget)
    {
        int previous = Progress.Target;
        if (previous == newTarget)
            return;

        Progress.Target = newTarget;
        await UpdateDiscoveryProgress();
    }

    public async Task UpdateActionProgress()
    {
        if (Progress.IsComplete)
        {
            await ReportFinalizedProgress();
            return;
        }

        await DisplayActionProgress();
    }
    private async Task DisplayActionProgress()
    {
        await SetContentAsync(GetActionProgressMessage());
    }
    public async Task UpdateActionProgress(int newCurrent)
    {
        int previous = Progress.Current;
        if (previous == newCurrent)
            return;

        Progress.Current = newCurrent;
        await UpdateActionProgress();
    }

    /// <summary>Reports that the action has been finalized.</summary>
    /// <param name="deletionDelay">The milliseconds after which to delete this persistent message. Specify any negative number if you don't want the message to be deleted.</param>
    /// <returns>A <seealso cref="Task"/> representing the task of reporting the message's progress, and optionally awaiting for deleting the message.</returns>
    /// <remarks>If the message is chosen to be deleted, this object is still not invalidated; you can still update it and a new message will be sent to the same channel.</remarks>
    public async Task ReportFinalizedProgress(int deletionDelay = -1)
    {
        await SetContentAsync(GetFinalizationMessage());
        
        if (deletionDelay < 0)
            return;

        await Task.Delay(deletionDelay);
        await DeleteAsync();
    }

    public async Task KeepUpdatingDiscoveryMessage(int refreshDelay, BoxedStruct<bool> complete)
    {
        bool updated = false;
        Progress.Updated += () => updated = true;

        while (!complete.Value)
        {
            if (updated)
            {
                updated = false;
                await UpdateDiscoveryProgress();
            }

            if (complete.Value)
                break;

            await Task.Delay(refreshDelay);
        }
    }
    public async Task KeepUpdatingProgressMessage(int refreshDelay, bool reportFinalizedProgress, int deletionDelay = -1)
    {
        bool updated = false;
        Progress.Updated += () => updated = true;
        
        while (!Progress.IsComplete)
        {
            if (updated)
            {
                updated = false;
                await DisplayActionProgress();
            }

            if (Progress.IsComplete)
                break;

            await Task.Delay(refreshDelay);
        }

        if (reportFinalizedProgress)
            await ReportFinalizedProgress(deletionDelay);
    }
}

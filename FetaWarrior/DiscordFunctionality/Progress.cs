using System;
using System.Threading;

namespace FetaWarrior.DiscordFunctionality;

public class Progress
{
    private int current;
    private int target;

    public int Current
    {
        get => current;
        set => UpdateField(ref current, value);
    }
    public int Target
    {
        get => target;
        set => UpdateField(ref target, value);
    }

    public event Action Updated;

    public double Ratio => (double)current / target;
    public double Percentage => Ratio * 100;

    public bool IsComplete => current == target;

    public Progress() { }
    public Progress(int target)
        : this(0, target) { }
    public Progress(int current, int target)
    {
        this.current = current;
        this.target = target;
    }

    private void UpdateField(ref int field, int value)
{
        int original = Interlocked.Exchange(ref field, value);
        if (original == value)
            return;
        
        Updated?.Invoke();
    }
}

using System;
using System.Diagnostics;

namespace FetaWarrior.Extensions;

public static class ProcessExtensions
{
    public static TimeSpan GetElapsedTime(this Process process)
    {
        return DateTime.UtcNow - process.StartTime.ToUniversalTime();
    }
}

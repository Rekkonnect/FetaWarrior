using System;
using System.Collections;

namespace FetaWarrior.Extensions;

public static class IEnumerableExtensions
{
    public static void ForEachObject(this IEnumerable enumerable, Action<object> action)
    {
        foreach (var value in enumerable)
            action(value);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace FetaWarrior.Extensions
{
    public static class IEnumerableExtensions
    {
        public static void Dissect<T>(this IEnumerable<T> collection, Predicate<T> predicate, out IEnumerable<T> matched, out IEnumerable<T> unmatched)
        {
            if (!collection.Any())
            {
                matched = Enumerable.Empty<T>();
                unmatched = Enumerable.Empty<T>();
                return;
            }

            var matchedList = new List<T>();
            var unmatchedList = new List<T>();

            matched = matchedList;
            unmatched = unmatchedList;

            foreach (var e in collection)
            {
                if (predicate(e))
                    matchedList.Add(e);
                else
                    unmatchedList.Add(e);
            }
        }
    }
}

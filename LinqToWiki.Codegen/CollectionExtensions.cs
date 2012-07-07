using System;
using System.Collections.Generic;

namespace LinqToWiki.Codegen
{
    /// <summary>
    /// Extension methods for collections
    /// </summary>
    static class CollectionExtensions
    {
        /// <summary>
        /// Removes items matching <see cref="predicate"/> from <see cref="list"/>
        /// and returns them in a new list.
        /// </summary>
        public static IList<T> RemoveAndReturn<T>(this IList<T> list, Func<T, bool> predicate)
        {
            var result = new List<T>();

            int i = 0;
            while (i < list.Count)
            {
                var item = list[i];
                if (predicate(item))
                {
                    result.Add(item);
                    list.RemoveAt(i);
                }
                else
                    i++;
            }

            return result;
        }

        /// <summary>
        /// Returns a collection that has
        /// the given object added after each item in the original collection.
        /// </summary>
        public static IEnumerable<T> AddAfterEach<T>(this IEnumerable<T> collection, T added)
        {
            foreach (var item in collection)
            {
                yield return item;
                yield return added;
            }
        }
    }
}
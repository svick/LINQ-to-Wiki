using System;
using System.Collections.Generic;

namespace LinqToWiki.Codegen
{
    /// <summary>
    /// Extension methods for <see cref="IList{T}"/>.
    /// </summary>
    static class ListExtensions
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
    }
}
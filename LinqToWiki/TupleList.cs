using System;
using System.Collections.Generic;

namespace LinqToWiki
{
    /// <summary>
    /// List of pairs of items.
    /// </summary>
    public class TupleList<T1, T2> : List<Tuple<T1, T2>>
    {
        public TupleList()
        {}

        public TupleList(IEnumerable<Tuple<T1, T2>> collection)
            : base(collection)
        {}

        /// <summary>
        /// Adds a pair of items to the end of the list.
        /// </summary>
        public void Add(T1 item1, T2 item2)
        {
            Add(Tuple.Create(item1, item2));
        }
    }
}
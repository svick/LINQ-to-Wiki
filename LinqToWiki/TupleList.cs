using System;
using System.Collections.Generic;

namespace LinqToWiki
{
    /// <summary>
    /// List of pairs of items.
    /// </summary>
    public class TupleList<T1, T2> : List<Tuple<T1, T2>>
    {
        private static readonly EqualityComparer<T1> KeyEqualityComparer = EqualityComparer<T1>.Default;

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

        public T2 this[T1 key]
        {
            get
            {
                return this.Find(x => KeyEqualityComparer.Equals(x.Item1, key)).Item2;
            }
            set
            {
                var index = this.FindIndex(x => KeyEqualityComparer.Equals(x.Item1, key));
                if (index < 0)
                    throw new InvalidOperationException();
                this[index] = Tuple.Create(key, value);
            }
        }
    }
}
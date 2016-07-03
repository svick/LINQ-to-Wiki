using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LinqToWiki.Collections
{
    /// <summary>
    /// List of pairs of items.
    /// In contrast with <see cref="Dictionary{TKey,TValue}"/>, a collection of this type is ordered.
    /// </summary>
    public class TupleList<T1, T2> : List<Tuple<T1, T2>>
    {
        private static readonly EqualityComparer<T1> KeyEqualityComparer = EqualityComparer<T1>.Default;

        /// <summary>
        /// Creates an empty list.
        /// </summary>
        public TupleList()
        {}

        /// <summary>
        /// Creates a list based on another collection.
        /// </summary>
        /// <param name="collection"></param>
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

        /// <summary>
        /// Finds the first tuple with given <see cref="Tuple{T1,T2}.Item1"/>
        /// and returns its <see cref="Tuple{T1,T2}.Item2"/>.
        /// 
        /// The set accesor finds first item with given <see cref="Tuple{T1,T2}.Item1"/>
        /// and replaces it with new <see cref="Tuple{T1,T2}"/> containing <c>value</c>
        /// as <see cref="Tuple{T1,T2}.Item2"/>.
        /// </summary>
        public T2 this[T1 key]
        {
            get
            {
                var tuple = this.Find(x => KeyEqualityComparer.Equals(x.Item1, key));

                Contract.Assume(tuple != null);

                return tuple.Item2;
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
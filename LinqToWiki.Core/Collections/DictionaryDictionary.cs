using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LinqToWiki.Collections
{
    /// <summary>
    /// Dictionary with composite key.
    /// Slightly simpler to use than <c>Dictionary&lt;TKey1, Dictionary&lt;TKey2, TValue&gt;&gt;</c>,
    /// because the inner dictionary can be created automatically.
    /// </summary>
    public class DictionaryDictionary<TKey1, TKey2, TValue> : Dictionary<TKey1, Dictionary<TKey2, TValue>>
    {
        /// <summary>
        /// Gets or sets the value associated with the specified composite key.
        /// </summary>
        public TValue this[TKey1 key1, TKey2 key2]
        {
            get
            {
                Contract.Requires(key1 != null);
                Contract.Requires(key2 != null);

                TValue result;
                if (TryGetValue(key1, key2, out result))
                    return result;
                throw new KeyNotFoundException();
            }
            set
            {
                Contract.Requires(key1 != null);
                Contract.Requires(key2 != null);

                GetOrCreateInnerDictionary(key1)[key2] = value;
            }
        }

        /// <summary>
        /// Returns the inner dictionary for the given key.
        /// If it doesn't exist yet, creates it first.
        /// </summary>
        private Dictionary<TKey2, TValue> GetOrCreateInnerDictionary(TKey1 key1)
        {
            Contract.Requires(key1 != null);

            Dictionary<TKey2, TValue> innerDict;
            bool found = TryGetValue(key1, out innerDict);
            if (!found)
            {
                innerDict = new Dictionary<TKey2, TValue>();
                Add(key1, innerDict);
            }
            return innerDict;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <returns><c>>true</c> if the Dictionary contains an element with the specified key; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(TKey1 key1, TKey2 key2, out TValue value)
        {
            Contract.Requires(key1 != null);
            Contract.Requires(key2 != null);

            Dictionary<TKey2, TValue> innerDict;
            bool found = TryGetValue(key1, out innerDict);
            if (found)
                return innerDict.TryGetValue(key2, out value);

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        public void Add(TKey1 key1, TKey2 key2, TValue value)
        {
            Contract.Requires(key1 != null);
            Contract.Requires(key2 != null);

            GetOrCreateInnerDictionary(key1).Add(key2, value);
        }
    }
}
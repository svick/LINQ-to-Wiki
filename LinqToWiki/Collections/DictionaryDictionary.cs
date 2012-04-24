using System.Collections.Generic;

namespace LinqToWiki.Collections
{
    /// <summary>
    /// Dictionary with composite key.
    /// Slightly simpler to use than <c>Dictionary&lt;TKey1, Dictionary&lt;TKey2, TValue&gt;&gt;</c>,
    /// because the inner dictionary can be created automatically.
    /// </summary>
    public class DictionaryDictionary<TKey1, TKey2, TValue> : Dictionary<TKey1, Dictionary<TKey2, TValue>>
    {
        public TValue this[TKey1 key1, TKey2 key2]
        {
            get
            {
                TValue result;
                if (TryGetValue(key1, key2, out result))
                    return result;
                throw new KeyNotFoundException();
            }
            set
            {
                GetOrCreateInnerDict(key1)[key2] = value;
            }
        }

        private Dictionary<TKey2, TValue> GetOrCreateInnerDict(TKey1 key1)
        {
            Dictionary<TKey2, TValue> innerDict;
            bool found = TryGetValue(key1, out innerDict);
            if (!found)
            {
                innerDict = new Dictionary<TKey2, TValue>();
                Add(key1, innerDict);
            }
            return innerDict;
        }

        public bool TryGetValue(TKey1 key1, TKey2 key2, out TValue value)
        {
            Dictionary<TKey2, TValue> innerDict;
            bool found = TryGetValue(key1, out innerDict);
            if (found)
                return innerDict.TryGetValue(key2, out value);

            value = default(TValue);
            return false;
        }

        public void Add(TKey1 key1, TKey2 key2, TValue value)
        {
            GetOrCreateInnerDict(key1).Add(key2, value);
        }
    }
}
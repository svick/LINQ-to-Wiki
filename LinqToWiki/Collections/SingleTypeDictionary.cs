using System;
using System.Collections.Generic;

namespace LinqToWiki.Collections
{
    // used to work around the fact that Roslyn doesn't support collection initializers
    public class SingleTypeDictionary<T> : Dictionary<T, T>
    {
         public SingleTypeDictionary(params T[] items)
         {
             if (items.Length % 2  == 1)
                 throw new ArgumentException("items");

             for (int i = 0; i < items.Length; i+=2)
             {
                 Add(items[i], items[i + 1]);
             }
         }
    }
}
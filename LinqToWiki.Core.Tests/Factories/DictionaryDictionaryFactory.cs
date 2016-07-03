using LinqToWiki.Collections;
// <copyright file="DictionaryDictionaryFactory.cs">Copyright ©  2011</copyright>

using System;
using System.Collections.Generic;
using Microsoft.Pex.Framework;

namespace LinqToWiki.Collections
{
    /// <summary>A factory for LinqToWiki.Collections.DictionaryDictionary`3[System.Int32,System.Int32,System.Int32] instances</summary>
    public static partial class DictionaryDictionaryFactory
    {
        /// <summary>A factory for LinqToWiki.Collections.DictionaryDictionary`3[System.Int32,System.Int32,System.Int32] instances</summary>
        [PexFactoryMethod(typeof(DictionaryDictionary<int, int, int>))]
        public static DictionaryDictionary<int, int, int> Create(IEnumerable<Tuple<int, int, int>> values)
        {
            DictionaryDictionary<int, int, int> dictionaryDictionary
               = new DictionaryDictionary<int, int, int>();

            foreach (var value in values)
            {
                dictionaryDictionary[value.Item1, value.Item2] = value.Item3;
            }

            return dictionaryDictionary;
        }
    }
}

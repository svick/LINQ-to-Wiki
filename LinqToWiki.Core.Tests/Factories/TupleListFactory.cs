using System.Collections.Generic;
using LinqToWiki.Collections;
// <copyright file="TupleListFactory.cs">Copyright ©  2011</copyright>

using System;
using Microsoft.Pex.Framework;

namespace LinqToWiki.Collections
{
    /// <summary>A factory for LinqToWiki.Collections.TupleList`2[System.Int32,System.Int32] instances</summary>
    public static partial class TupleListFactory
    {
        /// <summary>A factory for LinqToWiki.Collections.TupleList`2[System.Int32,System.Int32] instances</summary>
        [PexFactoryMethod(typeof(TupleList<int, int>))]
        public static TupleList<int, int> Create(IEnumerable<Tuple<int, int>> collection_iEnumerable)
        {
            TupleList<int, int> tupleList = new TupleList<int, int>(collection_iEnumerable);
            return tupleList;
        }
    }
}

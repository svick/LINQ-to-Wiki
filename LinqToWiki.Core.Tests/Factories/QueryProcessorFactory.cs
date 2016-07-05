using LinqToWiki.Internals;
// <copyright file="QueryProcessorFactory.cs">Copyright ©  2011</copyright>

using System;
using Microsoft.Pex.Framework;

namespace LinqToWiki.Internals
{
    /// <summary>A factory for LinqToWiki.Internals.QueryProcessor`1[System.Int32] instances</summary>
    public static partial class QueryProcessorFactory
    {
        /// <summary>A factory for LinqToWiki.Internals.QueryProcessor`1[System.Int32] instances</summary>
        [PexFactoryMethod(typeof(QueryProcessor<int>))]
        public static QueryProcessor<int> Create(
            WikiInfo wiki_wikiInfo,
            QueryTypeProperties<int> queryTypeProperties_queryTypeProperties
        )
        {
            QueryProcessor<int> queryProcessor = new QueryProcessor<int>
                                                     (wiki_wikiInfo, queryTypeProperties_queryTypeProperties);
            return queryProcessor;
        }
    }
}

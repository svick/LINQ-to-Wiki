using LinqToWiki.Internals;
// <copyright file="QueryPageProcessorFactory.cs">Copyright ©  2011</copyright>

using System;
using Microsoft.Pex.Framework;

namespace LinqToWiki.Internals
{
    /// <summary>A factory for LinqToWiki.Internals.QueryPageProcessor instances</summary>
    public static partial class QueryPageProcessorFactory
    {
        /// <summary>A factory for LinqToWiki.Internals.QueryPageProcessor instances</summary>
        [PexFactoryMethod(typeof(QueryPageProcessor))]
        public static QueryPageProcessor Create(WikiInfo wiki_wikiInfo)
        {
            QueryPageProcessor queryPageProcessor = new QueryPageProcessor(wiki_wikiInfo);
            return queryPageProcessor;
        }
    }
}

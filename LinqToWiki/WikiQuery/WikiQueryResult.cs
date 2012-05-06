using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Internals;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    /// <summary>
    /// Represents the result of a query.
    /// The query is actually executed only after <see cref="ToEnumerable"/>
    /// is called and its result is enumerated.
    /// </summary>
    public class WikiQueryResult<TSource, TResult> : IWikiQueryResult
    {
        protected QueryProcessor<TSource> QueryProcessor { get; private set; }
        protected QueryParameters<TSource, TResult> Parameters { get; private set; }

        QueryParameters IWikiQueryResult.Parameters
        {
            get { return Parameters; }
        }

        public WikiQueryResult(QueryProcessor<TSource> queryProcessor, QueryParameters<TSource, TResult> parameters)
        {
            QueryProcessor = queryProcessor;
            Parameters = parameters;
        }

        /// <summary>
        /// Exectes the query as a <see cref="List{T}"/>.
        /// </summary>
        public List<TResult> ToList()
        {
            return ToEnumerable().ToList();
        }

        /// <summary>
        /// Executes the query as an <see cref="IEnumerable{T}"/>.
        /// </summary>
        public IEnumerable<TResult> ToEnumerable()
        {
            return QueryProcessor.ExecuteList(Parameters);
        }
    }
}
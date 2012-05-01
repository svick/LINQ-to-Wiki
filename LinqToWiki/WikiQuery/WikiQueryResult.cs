using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Internals;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    /// <summary>
    /// Represents a result of a query.
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

        public List<TResult> ToList()
        {
            return ToEnumerable().ToList();
        }

        public IEnumerable<TResult> ToEnumerable()
        {
            return QueryProcessor.ExecuteList(Parameters);
        }
    }
}
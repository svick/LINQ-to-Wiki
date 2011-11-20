using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    public class WikiQueryResult<TSource, TResult>
    {
        protected Wiki Wiki { get; private set; }
        protected QueryParameters<TSource, TResult> Parameters { get; private set; }

        public WikiQueryResult(Wiki wiki, QueryParameters<TSource, TResult> parameters)
        {
            Wiki = wiki;
            Parameters = parameters;
        }

        public List<TResult> ToList()
        {
            return ToEnumerable().ToList();
        }

        public IEnumerable<TResult> ToEnumerable()
        {
            throw new NotImplementedException();
        }
    }
}
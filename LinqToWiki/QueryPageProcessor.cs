using System;
using System.Collections.Generic;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    public class QueryPageProcessor<TPage>
    {
        private readonly WikiInfo m_wiki;

        public QueryPageProcessor(WikiInfo wiki)
        {
            m_wiki = wiki;
        }

        public IEnumerable<TResult> ExecuteList<TResult>(PageQueryParameters parameters, Func<TPage, TResult> selector)
        {
            throw new NotImplementedException();
        }
    }
}
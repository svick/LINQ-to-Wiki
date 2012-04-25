using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    /// <summary>
    /// Represents a result of a “props” query.
    /// </summary>
    public class WikiQueryPageResult<TPage, TResult>
    {
        private readonly QueryPageProcessor<TPage> m_queryProcessor;
        private readonly PageQueryParameters m_parameters;
        private readonly Func<TPage, TResult> m_selector;

        public WikiQueryPageResult(QueryPageProcessor<TPage> queryProcessor, PageQueryParameters parameters, Func<TPage, TResult> selector)
        {
            m_selector = selector;
            m_queryProcessor = queryProcessor;
            m_parameters = parameters;
        }

        public List<TResult> ToList()
        {
            return ToEnumerable().ToList();
        }

        public IEnumerable<TResult> ToEnumerable()
        {
            return m_queryProcessor.ExecuteList(m_parameters, m_selector);
        }
    }
}
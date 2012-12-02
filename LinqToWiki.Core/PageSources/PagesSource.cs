using System;
using System.Linq.Expressions;
using LinqToWiki.Expressions;
using LinqToWiki.Internals;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    /// <summary>
    /// A page source, used in queries about a set of pages,
    /// where the page source represents the set.
    /// Similar to <see cref="System.Collections.Generic.IEnumerable{T}"/>,
    /// when compared with <see cref="IPagesCollection"/>.
    /// </summary>
    public abstract class PagesSource<TPage>
    {
        private readonly QueryPageProcessor m_queryPageProcessor;

        protected PagesSource(QueryPageProcessor queryPageProcessor)
        {
            m_queryPageProcessor = queryPageProcessor;
        }

        /// <summary>
        /// The object that can be used to create parameters for queries for the set of pages.
        /// </summary>
        protected abstract IPagesCollection GetPagesCollection();

        /// <summary>
        /// Retrieves the selected information for each page in this page source.
        /// </summary>
        public WikiQueryPageResult<TResult> Select<TResult>(Expression<Func<TPage, TResult>> selector)
        {
            Func<PageData, TResult> processedSelector;
            var parameters = PageExpressionParser.ParseSelect(
                selector, new PageQueryParameters(GetPagesCollection()), out processedSelector);
            return new WikiQueryPageResult<TResult>(
                m_queryPageProcessor, parameters, processedSelector, PageProperties<TPage>.Properties);
        }
    }
}
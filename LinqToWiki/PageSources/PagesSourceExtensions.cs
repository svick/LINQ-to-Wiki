using System;
using System.Linq.Expressions;
using LinqToWiki.Expressions;
using LinqToWiki.Internals;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    public static class PagesSourceExtensions
    {
        public static WikiQueryPageResult<TResult> Select<TPage, TResult>(
            this PagesSource<TPage> pagesSource, Expression<Func<TPage, TResult>> selector)
        {
            var source = (IPagesSource)pagesSource;

            Func<PageData, TResult> processedSelector;
            var parameters = PageExpressionParser.ParseSelect(selector, new PageQueryParameters(source.GetPagesCollection()), out processedSelector);
            return new WikiQueryPageResult<TResult>(source.QueryPageProcessor, parameters, processedSelector, PageProperties<TPage>.Properties);
        }
    }
}
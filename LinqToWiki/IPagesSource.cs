using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LinqToWiki.Expressions;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    public interface IPagesSource<TPage>
    {
        IEnumerable<Tuple<string, string>> BaseParameters { get; }
        QueryPageProcessor<TPage> QueryPageProcessor { get; }
    }

    public static class PagesSourceExtensions
    {
        public static WikiQueryPageResult<TPage, TResult> Select<TPage, TResult>(
            this IPagesSource<TPage> pagesSource, Expression<Func<TPage, TResult>> selector)
        {
            var parameters = PageExpressionParser.ParseSelect(ref selector, new PageQueryParameters(pagesSource.BaseParameters));
            return new WikiQueryPageResult<TPage, TResult>(pagesSource.QueryPageProcessor, parameters, selector.Compile());
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LinqToWiki.Collections;
using LinqToWiki.Expressions;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    public interface IPagesSource<TPage>
    {
        IEnumerable<Tuple<string, string>> BaseParameters { get; }
        QueryPageProcessor<TPage> QueryPageProcessor { get; }
    }

    public class TitlesSource<TPage> : IPagesSource<TPage>
    {
        public TitlesSource(WikiInfo wiki, IEnumerable<string> titles)
        {
            QueryPageProcessor = new QueryPageProcessor<TPage>(wiki);
            BaseParameters = new TupleList<string, string> { { "titles", NameValueParameter.JoinValues(titles) } };
        }

        public IEnumerable<Tuple<string, string>> BaseParameters { get; private set; }

        public QueryPageProcessor<TPage> QueryPageProcessor { get; private set; }
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
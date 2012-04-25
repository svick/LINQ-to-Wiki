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
        QueryPageProcessor QueryPageProcessor { get; }
    }

    public class TitlesSource<TPage> : IPagesSource<TPage>
    {
        public TitlesSource(WikiInfo wiki, IEnumerable<string> titles)
        {
            QueryPageProcessor = new QueryPageProcessor(wiki);
            BaseParameters = new TupleList<string, string> { { "titles", NameValueParameter.JoinValues(titles) } };
        }

        public IEnumerable<Tuple<string, string>> BaseParameters { get; private set; }

        public QueryPageProcessor QueryPageProcessor { get; private set; }
    }

    public static class PagesSourceExtensions
    {
        public static WikiQueryPageResult<TPage, TResult> Select<TPage, TResult>(
            this IPagesSource<TPage> pagesSource, Expression<Func<TPage, TResult>> selector)
        {
            Func<PageData, TResult> processedSelector;
            var parameters = PageExpressionParser.ParseSelect(selector, new PageQueryParameters(pagesSource.BaseParameters), out processedSelector);
            return new WikiQueryPageResult<TPage, TResult>(pagesSource.QueryPageProcessor, parameters, processedSelector);
        }
    }
}
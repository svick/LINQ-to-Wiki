using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using LinqToWiki.Collections;
using LinqToWiki.Expressions;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    interface IPagesSource
    {
        IEnumerable<Tuple<string, string>> BaseParameters { get; }
        QueryPageProcessor QueryPageProcessor { get; }
    }

    public class PagesSource<TPage> : IPagesSource
    {
        protected PagesSource()
        {}

        public PagesSource(IEnumerable<Tuple<string, string>> baseParameters, QueryPageProcessor queryPageProcessor)
        {
            BaseParameters = baseParameters;
            QueryPageProcessor = queryPageProcessor;
        }

        protected IEnumerable<Tuple<string, string>> BaseParameters { get; set; }

        IEnumerable<Tuple<string, string>> IPagesSource.BaseParameters
        {
            get { return BaseParameters; }
        }

        protected QueryPageProcessor QueryPageProcessor { get; set; }

        QueryPageProcessor IPagesSource.QueryPageProcessor
        {
            get { return QueryPageProcessor; }
        }
    }

    public abstract class ListSourceBase<TPage> : PagesSource<TPage>
    {
        protected ListSourceBase(WikiInfo wiki)
        {
            QueryPageProcessor = new QueryPageProcessor(wiki);
        }
    }

    public class TitlesSource<TPage> : ListSourceBase<TPage>
    {
        public TitlesSource(WikiInfo wiki, IEnumerable<string> titles)
            : base(wiki)
        {
            BaseParameters = new TupleList<string, string> { { "titles", NameValueParameter.JoinValues(titles) } };
        }
    }

    public class PageIdsSource<TPage> : ListSourceBase<TPage>
    {
        public PageIdsSource(WikiInfo wiki, IEnumerable<long> pageIds)
            : base(wiki)
        {
            BaseParameters =
                new TupleList<string, string>
                {
                    {
                        "pageids",
                        NameValueParameter.JoinValues(pageIds.Select(id => id.ToString(CultureInfo.InvariantCulture)))
                    }
                };
        }
    }

    public class RevIdsSource<TPage> : ListSourceBase<TPage>
    {
        public RevIdsSource(WikiInfo wiki, IEnumerable<long> revIds)
            : base(wiki)
        {
            BaseParameters =
                new TupleList<string, string>
                {
                    {
                        "revids",
                        NameValueParameter.JoinValues(revIds.Select(id => id.ToString(CultureInfo.InvariantCulture)))
                    }
                };
        }
    }

    public static class PagesSourceExtensions
    {
        public static WikiQueryPageResult<TResult> Select<TPage, TResult>(
            this PagesSource<TPage> pagesSource, Expression<Func<TPage, TResult>> selector)
        {
            var source = (IPagesSource)pagesSource;

            Func<PageData, TResult> processedSelector;
            var parameters = PageExpressionParser.ParseSelect(selector, new PageQueryParameters(source.BaseParameters), out processedSelector);
            return new WikiQueryPageResult<TResult>(source.QueryPageProcessor, parameters, processedSelector, PageProperties<TPage>.Properties);
        }
    }
}
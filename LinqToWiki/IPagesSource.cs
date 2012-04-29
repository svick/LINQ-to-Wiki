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
    public interface IPagesSource<TPage>
    {
        IEnumerable<Tuple<string, string>> BaseParameters { get; }
        QueryPageProcessor QueryPageProcessor { get; }
    }

    public abstract class SourceBase<TPage> : IPagesSource<TPage>
    {
        protected SourceBase(WikiInfo wiki)
        {
            QueryPageProcessor = new QueryPageProcessor(wiki);
        }

        public IEnumerable<Tuple<string, string>> BaseParameters { get; protected set; }

        public QueryPageProcessor QueryPageProcessor { get; private set; }
    }

    public class TitlesSource<TPage> : SourceBase<TPage>
    {
        public TitlesSource(WikiInfo wiki, IEnumerable<string> titles)
            : base(wiki)
        {
            BaseParameters = new TupleList<string, string> { { "titles", NameValueParameter.JoinValues(titles) } };
        }
    }

    public class PageIdsSource<TPage> : SourceBase<TPage>
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

    public class RevIdsSource<TPage> : SourceBase<TPage>
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
            this IPagesSource<TPage> pagesSource, Expression<Func<TPage, TResult>> selector)
        {
            Func<PageData, TResult> processedSelector;
            var parameters = PageExpressionParser.ParseSelect(selector, new PageQueryParameters(pagesSource.BaseParameters), out processedSelector);
            return new WikiQueryPageResult<TResult>(pagesSource.QueryPageProcessor, parameters, processedSelector, PageProperties<TPage>.Properties);
        }
    }
}
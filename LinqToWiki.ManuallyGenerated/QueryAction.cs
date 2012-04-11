using System.Collections.Generic;
using LinqToWiki.Collections;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    public class QueryAction
    {
        private readonly WikiInfo m_wiki;

        internal QueryAction(WikiInfo wiki)
        {
            m_wiki = wiki;
        }

        private static readonly TupleList<string, string> QueryBaseParameters =
            new TupleList<string, string> { { "action", "query" } };

        private static readonly IDictionary<string, string[]> CategoryMembersProps =
            new Dictionary<string, string[]>
            { { "pageid", new[] { "ids" } }, { "title", new[] { "title" } }, { "sortkey", new[] { "sortkey" } } };

        private static readonly QueryTypeProperties<CategoryMembersSelect> CategoryMembersProperties =
            new QueryTypeProperties<CategoryMembersSelect>(
                "categorymembers", "cm", QueryType.List,
                new TupleList<string, string>(QueryBaseParameters) { { "list", "categorymembers" } },
                CategoryMembersProps,
                CategoryMembersSelect.Parse);

        public WikiQuerySortable<CategoryMembersWhere, CategoryMembersOrderBy, CategoryMembersSelect> CategoryMembers(
            string title)
        {
            var parameters = QueryParameters.Create<CategoryMembersSelect>()
                .AddSingleValue("title", title);
            return new WikiQuerySortable<CategoryMembersWhere, CategoryMembersOrderBy, CategoryMembersSelect>(
                new QueryProcessor<CategoryMembersSelect>(m_wiki, CategoryMembersProperties),
                parameters);
        }
    }
}
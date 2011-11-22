using System.Collections.Generic;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    public class QueryAction
    {
        private readonly Wiki m_wiki;

        internal QueryAction(Wiki wiki)
        {
            m_wiki = wiki;
        }

        private static readonly TupleList<string, string> QueryBaseParameters =
            new TupleList<string, string> { { "action", "query" } };

        private static readonly QueryTypeProperties<CategoryMembersSelect> CategoryMembersProperties =
            new QueryTypeProperties<CategoryMembersSelect>(
                "cm", new TupleList<string, string>(QueryBaseParameters) { { "list", "categorymembers" } },
                new Dictionary<string, string> { { "pageid", "ids" }, { "title", "title" }, { "sortkey", "sortkey" } },
                CategoryMembersSelect.Parse);

        public WikiQuery<CategoryMembersWhere, CategoryMembersOrderBy, CategoryMembersSelect> CategoryMembers(
            string title)
        {
            var parameters = QueryParameters.Create<CategoryMembersSelect>()
                .AddSingleValue("title", title);
            return new WikiQuery<CategoryMembersWhere, CategoryMembersOrderBy, CategoryMembersSelect>(
                new QueryProcessor<CategoryMembersSelect>(m_wiki, CategoryMembersProperties),
                parameters);
        }
    }
}
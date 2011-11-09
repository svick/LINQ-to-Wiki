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

        public WikiQuery<CategoryMembersWhere, CategoryMembersOrderBy, CategoryMembersSelect> CategoryMembers(string title)
        {
            var parameters = QueryParameter.WithAction(Parameters.QueryAction.Query)
                .AddType(QueryType.CategoryMembers)
                .AddSingleValue("title", title);
            return new WikiQuery<CategoryMembersWhere, CategoryMembersOrderBy, CategoryMembersSelect>(m_wiki, parameters);
        }
    }
}
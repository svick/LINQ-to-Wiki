using System;
using System.Linq.Expressions;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    public class WikiQuery<TWhere, TOrderBy, TSelect>
    {
        private readonly Wiki m_wiki;
        private readonly QueryParameter m_parameters;

        public WikiQuery(Wiki wiki, QueryParameter parameters)
        {
            m_wiki = wiki;
            m_parameters = parameters;
        }

        public WikiQuery<TWhere, TOrderBy, TSelect> Where(Expression<Func<TWhere, bool>> predicate)
        {
            WhereExpressionParser.Parse(predicate, m_parameters);
            return this;
        }

        public WikiQuery<TWhere, TOrderBy, TSelect> OrderBy<TResult>(Expression<Func<TOrderBy, TResult>> keySelector)
        {
            return this;
        }

        public WikiQuery<TWhere, TOrderBy, TSelect> OrderByDescending<TResult>(Expression<Func<TOrderBy, TResult>> selector)
        {
            return this;
        }

        public WikiQueryResult<TResult> Select<TResult>(Expression<Func<TSelect, TResult>> selector)
        {
            return new WikiQueryResult<TResult>();
        }
    }
}
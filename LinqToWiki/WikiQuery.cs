using System;
using System.Linq.Expressions;
using LinqToWiki.Expressions;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    public class WikiQuery<TWhere, TOrderBy, TSelect> : WikiQueryResult<TSelect, TSelect>
    {
        public WikiQuery(Wiki wiki, QueryParameters<TSelect, TSelect> parameters)
            : base(wiki, parameters)
        {}

        public WikiQuery<TWhere, TOrderBy, TSelect> Where(Expression<Func<TWhere, bool>> predicate)
        {
            return new WikiQuery<TWhere, TOrderBy, TSelect>(Wiki, ExpressionParser.ParseWhere(predicate, Parameters));
        }

        public WikiQuery<TWhere, TOrderBy, TSelect> OrderBy<TKey>(Expression<Func<TOrderBy, TKey>> keySelector)
        {
            return new WikiQuery<TWhere, TOrderBy, TSelect>(Wiki, ExpressionParser.ParseOrderBy(keySelector, Parameters, true));
        }

        public WikiQuery<TWhere, TOrderBy, TSelect> OrderByDescending<TKey>(Expression<Func<TOrderBy, TKey>> keySelector)
        {
            return new WikiQuery<TWhere, TOrderBy, TSelect>(Wiki, ExpressionParser.ParseOrderBy(keySelector, Parameters, false));
        }

        public WikiQueryResult<TSelect, TResult> Select<TResult>(Expression<Func<TSelect, TResult>> selector)
        {
            return new WikiQueryResult<TSelect, TResult>(Wiki, ExpressionParser.ParseSelect(selector, Parameters));
        }
    }
}
using System;
using System.Linq.Expressions;
using LinqToWiki.Expressions;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    /// <summary>
    /// Represents a query.
    /// </summary>
    public class WikiQuery<TWhere, TOrderBy, TSelect> : WikiQueryResult<TSelect, TSelect>
    {
        public WikiQuery(QueryProcessor<TSelect> queryProcessor, QueryParameters<TSelect, TSelect> parameters)
            : base(queryProcessor, parameters)
        {}

        public WikiQuery<TWhere, TOrderBy, TSelect> Where(Expression<Func<TWhere, bool>> predicate)
        {
            return new WikiQuery<TWhere, TOrderBy, TSelect>(QueryProcessor, ExpressionParser.ParseWhere(predicate, Parameters));
        }

        public WikiQuery<TWhere, TOrderBy, TSelect> OrderBy<TKey>(Expression<Func<TOrderBy, TKey>> keySelector)
        {
            return new WikiQuery<TWhere, TOrderBy, TSelect>(QueryProcessor, ExpressionParser.ParseOrderBy(keySelector, Parameters, true));
        }

        public WikiQuery<TWhere, TOrderBy, TSelect> OrderByDescending<TKey>(Expression<Func<TOrderBy, TKey>> keySelector)
        {
            return new WikiQuery<TWhere, TOrderBy, TSelect>(QueryProcessor, ExpressionParser.ParseOrderBy(keySelector, Parameters, false));
        }

        public WikiQueryResult<TSelect, TResult> Select<TResult>(Expression<Func<TSelect, TResult>> selector)
        {
            return new WikiQueryResult<TSelect, TResult>(QueryProcessor, ExpressionParser.ParseSelect(selector, Parameters));
        }
    }
}
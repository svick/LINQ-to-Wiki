using System;
using System.Linq.Expressions;
using LinqToWiki.Expressions;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    /// <summary>
    /// Represents a sortable query.
    /// </summary>
    public class WikiQuerySortable<TWhere, TOrderBy, TSelect> : WikiQueryResult<TSelect, TSelect>
    {
        public WikiQuerySortable(QueryProcessor<TSelect> queryProcessor, QueryParameters<TSelect, TSelect> parameters)
            : base(queryProcessor, parameters)
        {}

        public WikiQuerySortable<TWhere, TOrderBy, TSelect> Where(Expression<Func<TWhere, bool>> predicate)
        {
            return new WikiQuerySortable<TWhere, TOrderBy, TSelect>(QueryProcessor, ExpressionParser.ParseWhere(predicate, Parameters));
        }

        public WikiQuery<TWhere, TSelect> OrderBy<TKey>(Expression<Func<TOrderBy, TKey>> keySelector)
        {
            return new WikiQuery<TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseOrderBy(keySelector, Parameters, true));
        }

        public WikiQuery<TWhere, TSelect> OrderByDescending<TKey>(Expression<Func<TOrderBy, TKey>> keySelector)
        {
            return new WikiQuery<TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseOrderBy(keySelector, Parameters, false));
        }

        public WikiQuerySortable<TWhere, TOrderBy, TSelect> Select(Expression<Func<TSelect, TSelect>> selector)
        {
            return new WikiQuerySortable<TWhere, TOrderBy, TSelect>(QueryProcessor, ExpressionParser.ParseIdentitySelect(selector, Parameters));
        }

        public WikiQueryResult<TSelect, TResult> Select<TResult>(Expression<Func<TSelect, TResult>> selector)
        {
            return new WikiQueryResult<TSelect, TResult>(QueryProcessor, ExpressionParser.ParseSelect(selector, Parameters));
        }
    }

    /// <summary>
    /// Represents a nonsortable query.
    /// </summary>
    public class WikiQuery<TWhere, TSelect> : WikiQueryResult<TSelect, TSelect>
    {
        public WikiQuery(QueryProcessor<TSelect> queryProcessor, QueryParameters<TSelect, TSelect> parameters)
            : base(queryProcessor, parameters)
        {}

        public WikiQuery<TWhere, TSelect> Where(Expression<Func<TWhere, bool>> predicate)
        {
            return new WikiQuery<TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseWhere(predicate, Parameters));
        }

        public WikiQueryResult<TSelect, TResult> Select<TResult>(Expression<Func<TSelect, TResult>> selector)
        {
            return new WikiQueryResult<TSelect, TResult>(QueryProcessor, ExpressionParser.ParseSelect(selector, Parameters));
        }

        public WikiQuery<TWhere, TSelect> Select(Expression<Func<TSelect, TSelect>> selector)
        {
            return new WikiQuery<TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseIdentitySelect(selector, Parameters));
        }
    }
}
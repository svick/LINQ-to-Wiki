using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LinqToWiki.Expressions;
using LinqToWiki.Internals;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    /// <summary>
    /// Represents a sortable query.
    /// </summary>
    public class WikiQuerySortable<TWhere, TOrderBy, TSelect> : WikiQuery<TWhere, TSelect>
    {
        public WikiQuerySortable(QueryProcessor<TSelect> queryProcessor, QueryParameters<TSelect, TSelect> parameters)
            : base(queryProcessor, parameters)
        {}

        /// <summary>
        /// Specify filter or other parameter of the query.
        /// </summary>
        public new WikiQuerySortable<TWhere, TOrderBy, TSelect> Where(Expression<Func<TWhere, bool>> predicate)
        {
            Contract.Requires(predicate != null);

            return new WikiQuerySortable<TWhere, TOrderBy, TSelect>(QueryProcessor, ExpressionParser.ParseWhere(predicate, Parameters));
        }

        /// <summary>
        /// Specify that the query should be ordered ascending by the given key.
        /// </summary>
        public WikiQuery<TWhere, TSelect> OrderBy<TKey>(Expression<Func<TOrderBy, TKey>> keySelector)
        {
            Contract.Requires(keySelector != null);

            return new WikiQuery<TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseOrderBy(keySelector, Parameters, true));
        }

        /// <summary>
        /// Specify that the query should be ordered descending by the given key.
        /// </summary>
        public WikiQuery<TWhere, TSelect> OrderByDescending<TKey>(Expression<Func<TOrderBy, TKey>> keySelector)
        {
            Contract.Requires(keySelector != null);

            return new WikiQuery<TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseOrderBy(keySelector, Parameters, false));
        }

        /// <summary>
        /// Specify that the result should be the whole object.
        /// The parameter has to be an identity (e.g. <c>x => x</c>).
        /// </summary>
        public new WikiQuerySortable<TWhere, TOrderBy, TSelect> Select(Expression<Func<TSelect, TSelect>> selector)
        {
            Contract.Requires(selector != null);

            return new WikiQuerySortable<TWhere, TOrderBy, TSelect>(QueryProcessor, ExpressionParser.ParseIdentitySelect(selector, Parameters));
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

        /// <summary>
        /// Specify filter or other parameter of the query.
        /// </summary>
        public WikiQuery<TWhere, TSelect> Where(Expression<Func<TWhere, bool>> predicate)
        {
            Contract.Requires(predicate != null);

            return new WikiQuery<TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseWhere(predicate, Parameters));
        }

        /// <summary>
        /// Specify that the result should be the whole object.
        /// The parameter has to be an identity (e.g. <c>x => x</c>).
        /// </summary>
        public WikiQuery<TWhere, TSelect> Select(Expression<Func<TSelect, TSelect>> selector)
        {
            Contract.Requires(selector != null);

            return new WikiQuery<TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseIdentitySelect(selector, Parameters));
        }

        /// <summary>
        /// Selects the result object.
        /// </summary>
        public WikiQueryResult<TSelect, TResult> Select<TResult>(Expression<Func<TSelect, TResult>> selector)
        {
            Contract.Requires(selector != null);
            Contract.Requires(selector.Parameters.Count == 1);

            return new WikiQueryResult<TSelect, TResult>(QueryProcessor, ExpressionParser.ParseSelect(selector, Parameters));
        }
    }
}
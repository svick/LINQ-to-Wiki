using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using LinqToWiki.Expressions;
using LinqToWiki.Internals;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    /// <summary>
    /// Represents a sortable query that can be used as a generator.
    /// </summary>
    public class WikiQuerySortableGenerator<TPage, TWhere, TOrderBy, TSelect> : WikiQuerySortable<TWhere, TOrderBy, TSelect>
    {
        public WikiQuerySortableGenerator(QueryProcessor<TSelect> queryProcessor, QueryParameters<TSelect, TSelect> parameters)
            : base(queryProcessor, parameters)
        { }

        /// <summary>
        /// Specify filter or other parameter of the query.
        /// </summary>
        public new WikiQuerySortableGenerator<TPage, TWhere, TOrderBy, TSelect> Where(Expression<Func<TWhere, bool>> predicate)
        {
            Contract.Requires(predicate != null);

            return new WikiQuerySortableGenerator<TPage, TWhere, TOrderBy, TSelect>(QueryProcessor, ExpressionParser.ParseWhere(predicate, Parameters));
        }

        /// <summary>
        /// Specify that the query should be ordered ascending by the given key.
        /// </summary>
        public new WikiQueryGenerator<TPage, TWhere, TSelect> OrderBy<TKey>(Expression<Func<TOrderBy, TKey>> keySelector)
        {
            Contract.Requires(keySelector != null);

            return new WikiQueryGenerator<TPage, TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseOrderBy(keySelector, Parameters, true));
        }

        /// <summary>
        /// Specify that the query should be ordered descending by the given key.
        /// </summary>
        public new WikiQueryGenerator<TPage, TWhere, TSelect> OrderByDescending<TKey>(Expression<Func<TOrderBy, TKey>> keySelector)
        {
            Contract.Requires(keySelector != null);

            return new WikiQueryGenerator<TPage, TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseOrderBy(keySelector, Parameters, false));
        }

        /// <summary>
        /// Specify that the result should be the whole object.
        /// The parameter has to be an identity (e.g. <c>x => x</c>).
        /// </summary>
        public new WikiQuerySortableGenerator<TPage, TWhere, TOrderBy, TSelect> Select(Expression<Func<TSelect, TSelect>> selector)
        {
            Contract.Requires(selector != null);
            Contract.Requires(selector.Parameters.Count() == 1);

            return new WikiQuerySortableGenerator<TPage, TWhere, TOrderBy, TSelect>(QueryProcessor, ExpressionParser.ParseIdentitySelect(selector, Parameters));
        }

        /// <summary>
        /// Get generator page source for the current query, that can be used for further queries.
        /// </summary>
        public PagesSource<TPage> Pages
        {
            get
            {
                return new GeneratorPagesSource<TPage>(
                    QueryProcessor.ProcessGeneratorParameters(Parameters), QueryProcessor.GetPageProcessor());
            }
        }
    }

    /// <summary>
    /// Represents a nonsortable query that can be used as a generator
    /// </summary>
    public class WikiQueryGenerator<TPage, TWhere, TSelect> : WikiQuery<TWhere, TSelect>
    {
        public WikiQueryGenerator(QueryProcessor<TSelect> queryProcessor, QueryParameters<TSelect, TSelect> parameters)
            : base(queryProcessor, parameters)
        { }

        /// <summary>
        /// Specify filter or other parameter of the query.
        /// </summary>
        public new WikiQueryGenerator<TPage, TWhere, TSelect> Where(Expression<Func<TWhere, bool>> predicate)
        {
            Contract.Requires(predicate != null);

            return new WikiQueryGenerator<TPage, TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseWhere(predicate, Parameters));
        }

        /// <summary>
        /// Specify that the result should be the whole object.
        /// The parameter has to be an identity (e.g. <c>x => x</c>).
        /// </summary>
        public new WikiQueryGenerator<TPage, TWhere, TSelect> Select(Expression<Func<TSelect, TSelect>> selector)
        {
            Contract.Requires(selector != null);
            Contract.Requires(selector.Parameters.Count() == 1);

            return new WikiQueryGenerator<TPage, TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseIdentitySelect(selector, Parameters));
        }

        /// <summary>
        /// Get generator page source for the current query, that can be used for further queries.
        /// </summary>
        public PagesSource<TPage> Pages
        {
            get
            {
                return new GeneratorPagesSource<TPage>(
                    QueryProcessor.ProcessGeneratorParameters(Parameters), QueryProcessor.GetPageProcessor());
            }
        }
    }
}
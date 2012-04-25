using System;
using System.Linq.Expressions;
using LinqToWiki.Expressions;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    /// <summary>
    /// Represents a sortable query that can be used as a generator.
    /// </summary>
    public class WikiQuerySortableGenerator<TWhere, TOrderBy, TSelect> : WikiQuerySortable<TWhere, TOrderBy, TSelect>
    {
        public WikiQuerySortableGenerator(QueryProcessor<TSelect> queryProcessor, QueryParameters<TSelect, TSelect> parameters)
            : base(queryProcessor, parameters)
        { }

        public new WikiQuerySortableGenerator<TWhere, TOrderBy, TSelect> Where(Expression<Func<TWhere, bool>> predicate)
        {
            return new WikiQuerySortableGenerator<TWhere, TOrderBy, TSelect>(QueryProcessor, ExpressionParser.ParseWhere(predicate, Parameters));
        }

        public new WikiQueryGenerator<TWhere, TSelect> OrderBy<TKey>(Expression<Func<TOrderBy, TKey>> keySelector)
        {
            return new WikiQueryGenerator<TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseOrderBy(keySelector, Parameters, true));
        }

        public new WikiQueryGenerator<TWhere, TSelect> OrderByDescending<TKey>(Expression<Func<TOrderBy, TKey>> keySelector)
        {
            return new WikiQueryGenerator<TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseOrderBy(keySelector, Parameters, false));
        }

        public new WikiQuerySortableGenerator<TWhere, TOrderBy, TSelect> Select(Expression<Func<TSelect, TSelect>> selector)
        {
            return new WikiQuerySortableGenerator<TWhere, TOrderBy, TSelect>(QueryProcessor, ExpressionParser.ParseIdentitySelect(selector, Parameters));
        }

        /*IPagesSource Pages
        {
            get { return Parameters; }
        }*/
    }

    /// <summary>
    /// Represents a nonsortable query that can be used as a generator
    /// </summary>
    public class WikiQueryGenerator<TWhere, TSelect> : WikiQuery<TWhere, TSelect>
    {
        public WikiQueryGenerator(QueryProcessor<TSelect> queryProcessor, QueryParameters<TSelect, TSelect> parameters)
            : base(queryProcessor, parameters)
        { }

        public new WikiQueryGenerator<TWhere, TSelect> Where(Expression<Func<TWhere, bool>> predicate)
        {
            return new WikiQueryGenerator<TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseWhere(predicate, Parameters));
        }

        public new WikiQueryGenerator<TWhere, TSelect> Select(Expression<Func<TSelect, TSelect>> selector)
        {
            return new WikiQueryGenerator<TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseIdentitySelect(selector, Parameters));
        }
    }
}
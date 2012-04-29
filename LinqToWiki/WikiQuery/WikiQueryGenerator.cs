using System;
using System.Linq.Expressions;
using LinqToWiki.Expressions;
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

        public new WikiQuerySortableGenerator<TPage, TWhere, TOrderBy, TSelect> Where(Expression<Func<TWhere, bool>> predicate)
        {
            return new WikiQuerySortableGenerator<TPage, TWhere, TOrderBy, TSelect>(QueryProcessor, ExpressionParser.ParseWhere(predicate, Parameters));
        }

        public new WikiQueryGenerator<TPage, TWhere, TSelect> OrderBy<TKey>(Expression<Func<TOrderBy, TKey>> keySelector)
        {
            return new WikiQueryGenerator<TPage, TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseOrderBy(keySelector, Parameters, true));
        }

        public new WikiQueryGenerator<TPage, TWhere, TSelect> OrderByDescending<TKey>(Expression<Func<TOrderBy, TKey>> keySelector)
        {
            return new WikiQueryGenerator<TPage, TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseOrderBy(keySelector, Parameters, false));
        }

        public new WikiQuerySortableGenerator<TPage, TWhere, TOrderBy, TSelect> Select(Expression<Func<TSelect, TSelect>> selector)
        {
            return new WikiQuerySortableGenerator<TPage, TWhere, TOrderBy, TSelect>(QueryProcessor, ExpressionParser.ParseIdentitySelect(selector, Parameters));
        }

        public PagesSource<TPage> Pages
        {
            get
            {
                return new PagesSource<TPage>(
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

        public new WikiQueryGenerator<TPage, TWhere, TSelect> Where(Expression<Func<TWhere, bool>> predicate)
        {
            return new WikiQueryGenerator<TPage, TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseWhere(predicate, Parameters));
        }

        public new WikiQueryGenerator<TPage, TWhere, TSelect> Select(Expression<Func<TSelect, TSelect>> selector)
        {
            return new WikiQueryGenerator<TPage, TWhere, TSelect>(QueryProcessor, ExpressionParser.ParseIdentitySelect(selector, Parameters));
        }

        public PagesSource<TPage> Pages
        {
            get
            {
                return new PagesSource<TPage>(
                    QueryProcessor.ProcessGeneratorParameters(Parameters), QueryProcessor.GetPageProcessor());
            }
        }
    }
}
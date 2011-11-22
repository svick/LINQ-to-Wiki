using System;
using System.Collections.Generic;

namespace LinqToWiki.Parameters
{
    /// <summary>
    /// Non-generic part of <see cref="QueryParameters{TSource,TResult}"/>.
    /// </summary>
    public abstract class QueryParameters
    {
        /// <summary>
        /// Linked list of general parameters.
        /// </summary>
        public NameValuesParameter Values { get; protected set; }

        /// <summary>
        /// Property to sort by. Is represented by <c>sort</c> in the query.
        /// If <c>null</c>, no sorting is done.
        /// </summary>
        public string Sort { get; protected set; }

        /// <summary>
        /// Sort in ascending direction? Is represented by <c>dir</c> in the query.
        /// Is <c>null</c> iff <see cref="Sort"/> is <c>null</c>.
        /// </summary>
        public bool? Ascending { get; protected set; }

        /// <summary>
        /// Copies properties of this instance to <see cref="target"/>.
        /// Copies only properties of <see cref="QueryParameters"/> and not the inherited classes.
        /// </summary>
        protected void CopyTo(QueryParameters target)
        {
            target.Values = Values;
            target.Sort = Sort;
            target.Ascending = Ascending;
        }

        /// <summary>
        /// Creates new query parameters.
        /// </summary>
        public static QueryParameters<TSource, TSource> Create<TSource>()
        {
            return new QueryParametersWithoutSelect<TSource>();
        }
    }

    /// <summary>
    /// Represents parameters to a query.
    /// This class is immutable. Methods that modify the query return the modified one
    /// and don't affect the current object.
    /// </summary>
    /// <typeparam name="TSource">Type used to represent the source of the query.</typeparam>
    /// <typeparam name="TResult">Type of the item in the result collection.</typeparam>
    public abstract class QueryParameters<TSource, TResult> : QueryParameters
    {
        /// <summary>
        /// Properties of the source that should be included in the result.
        /// The value of <c>null</c> means that all properties should be included.
        /// </summary>
        public IEnumerable<string> Properties { get; protected set; }

        /// <summary>
        /// Function, that can be used to convert an instance of the source object to the result object.
        /// </summary>
        public Func<TSource, TResult> Selector { get; protected set; }

        /// <summary>
        /// Creates a clone of the current object without copying properties of <see cref="QueryParameters"/>.
        /// </summary>
        protected abstract QueryParameters<TSource, TResult> CloneNonShared();

        /// <summary>
        /// Creates a clone of the current object.
        /// </summary>
        protected QueryParameters<TSource, TResult> Clone()
        {
            var result = CloneNonShared();
            CopyTo(result);
            return result;
        }

        /// <summary>
        /// Adds a single name-value pair to the current query and returns the result.
        /// </summary>
        public QueryParameters<TSource, TResult> AddSingleValue(string name, string value)
        {
            var result = Clone();
            result.Values = new NameValuesParameter(Values, name, value);
            return result;
        }

        /// <summary>
        /// Adds sorting to the current query and returns the result.
        /// </summary>
        public QueryParameters<TSource, TResult> WithSort(string name, bool ascending)
        {
            var result = Clone();
            result.Sort = name;
            result.Ascending = ascending;
            return result;
        }

        /// <summary>
        /// Adds projection to the current query and returns the result.
        /// </summary>
        /// <param name="properties">
        /// Properties of the source that should be included in the result.
        /// The value of <c>null</c> means that all properties should be included.
        /// </param>
        /// <param name="selector">Function, that can be used to convert an instance of the source object to the result object.</param>
        public QueryParameters<TSource, TNewResult> WithSelect<TNewResult>(
            IEnumerable<string> properties, Func<TSource, TNewResult> selector)
        {
            var result = new QueryParametersWithSelect<TSource, TNewResult>(properties, selector);
            CopyTo(result);
            return result;
        }
    }

    /// <summary>
    /// Represents a query wihtout projection, the result is the same as the source.
    /// </summary>
    class QueryParametersWithoutSelect<TSource> : QueryParameters<TSource, TSource>
    {
        public QueryParametersWithoutSelect()
        {
            Selector = x => x;
        }

        /// <summary>
        /// Creates a clone of the current object without copying properties of <see cref="QueryParameters"/>.
        /// </summary>
        protected override QueryParameters<TSource, TSource> CloneNonShared()
        {
            return new QueryParametersWithoutSelect<TSource>();
        }
    }

    /// <summary>
    /// Represents a query with projection.
    /// </summary>
    class QueryParametersWithSelect<TSource, TResult> : QueryParameters<TSource, TResult>
    {
        internal QueryParametersWithSelect(IEnumerable<string> properties, Func<TSource, TResult> selector)
        {
            Selector = selector;
            Properties = properties;
        }

        /// <summary>
        /// Creates a clone of the current object without copying properties of <see cref="QueryParameters"/>.
        /// </summary>
        protected override QueryParameters<TSource, TResult> CloneNonShared()
        {
            return new QueryParametersWithSelect<TSource, TResult>(Properties, Selector);
        }
    }
}
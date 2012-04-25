using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqToWiki.Parameters
{
    /// <summary>
    /// Non-generic part of <see cref="QueryParameters{TSource,TResult}"/>.
    /// </summary>
    public abstract class QueryParameters
    {
        protected QueryParameters()
        {}

        /// <summary>
        /// Linked list of general parameters.
        /// </summary>
        public NameValueParameter Value { get; protected set; }

        /// <summary>
        /// Property to sort by. Is represented by <c>sort</c> in the query.
        /// </summary>
        public string Sort { get; protected set; }

        /// <summary>
        /// Sort in ascending direction? Is represented by <c>dir</c> in the query.
        /// If <c>null</c>, no sorting is done.
        /// </summary>
        public bool? Ascending { get; protected set; }

        /// <summary>
        /// Properties of the source that should be included in the result.
        /// The value of <c>null</c> means that all properties should be included.
        /// </summary>
        public IEnumerable<string> Properties { get; protected set; }

        /// <summary>
        /// Copies properties of this instance to <see cref="target"/>.
        /// Copies only properties of <see cref="QueryParameters"/> and not the inherited classes.
        /// </summary>
        protected void CopyTo(QueryParameters target)
        {
            target.Value = Value;
            target.Sort = Sort;
            target.Ascending = Ascending;
            target.Properties = Properties;
        }

        /// <summary>
        /// Creates new generic query parameters.
        /// </summary>
        public static QueryParameters<TSource, TSource> Create<TSource>()
        {
            return new QueryParameters<TSource, TSource>(x => x);
        }
    }

    /// <summary>
    /// Represents parameters to a query.
    /// This class is immutable. Methods that modify the query return the modified one
    /// and don't affect the current object.
    /// </summary>
    /// <typeparam name="TSource">Type used to represent the source of the query.</typeparam>
    /// <typeparam name="TResult">Type of the item in the result collection.</typeparam>
    public class QueryParameters<TSource, TResult> : QueryParameters
    {
        public QueryParameters(Func<TSource, TResult> selector)
        {
            Selector = selector;
        }

        /// <summary>
        /// Function, that can be used to convert an instance of the source object to the result object.
        /// </summary>
        public Func<TSource, TResult> Selector { get; private set; }

        /// <summary>
        /// Creates a clone of the current object.
        /// </summary>
        private QueryParameters<TSource, TResult> Clone()
        {
            var result = new QueryParameters<TSource, TResult>(Selector);
            CopyTo(result);
            return result;
        }

        /// <summary>
        /// Adds a single name-value pair to the current query and returns the result.
        /// The parameter <see cref="value"/> can actually represent multiple values.
        /// </summary>
        public QueryParameters<TSource, TResult> AddSingleValue(string name, string value)
        {
            if (value == null)
                return this;

            if (Value != null && Value.Any(v => v.Name == name))
                throw new InvalidOperationException(
                    string.Format("Tried adding value with the name '{0}' that is already present.", name));

            var result = Clone();
            result.Value = new NameValueParameter(Value, name, value);
            return result;
        }

        /// <summary>
        /// Adds a name-value pair to the current query and returns the result.
        /// </summary>
        public QueryParameters<TSource, TResult> AddMultipleValues(string name, IEnumerable<string> values)
        {
            return AddSingleValue(name, NameValueParameter.JoinValues(values));
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
            var result = new QueryParameters<TSource, TNewResult>(selector);
            CopyTo(result);
            result.Properties = properties;
            return result;
        }
    }
}
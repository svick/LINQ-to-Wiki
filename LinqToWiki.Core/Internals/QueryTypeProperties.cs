using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml.Linq;

namespace LinqToWiki.Internals
{
    /// <summary>
    /// Represents a module and its properties.
    /// Non-generic part of <see cref="QueryTypeProperties{T}"/>.
    /// </summary>
    public abstract class QueryTypeProperties
    {
        private readonly IDictionary<string, string[]> m_props;

        protected QueryTypeProperties(
            string moduleName, string prefix, QueryType? queryType, SortType? sortType,
            IEnumerable<Tuple<string, string>> baseParameters, IDictionary<string, string[]> props)
        {
            ModuleName = moduleName;
            Prefix = prefix;
            QueryType = queryType;
            SortType = sortType;
            BaseParameters = baseParameters;
            m_props = props ?? new Dictionary<string, string[]>();
        }

        /// <summary>
        /// Name of the module.
        /// </summary>
        public string ModuleName { get; private set; }

        /// <summary>
        /// Prefix that is used by this type of query.
        /// </summary>
        public string Prefix { get; private set; }

        /// <summary>
        /// Type of the query module, or <c>null</c>, if it's not a query module.
        /// </summary>
        public QueryType? QueryType { get; private set; }

        /// <summary>
        /// Whether <c>dir</c> parameter uses <c>ascending</c>/<c>descending</c> or <c>newer</c>/<c>older</c>.
        /// If <c>null</c>, module doesn't support sorting.
        /// </summary>
        public SortType? SortType { get; private set; }

        /// <summary>
        /// Parameters, that are included in all queries of this type.
        /// </summary>
        public IEnumerable<Tuple<string, string>> BaseParameters { get; private set; }

        /// <summary>
        /// Returns the value of <c>prop</c> for given property.
        /// </summary>
        public string[] GetProps(string property)
        {
            return m_props[Expressions.ExpressionParser.ReversePropertyName(property)];
        }

        /// <summary>
        /// Returns all possible values for <c>prop</c>.
        /// </summary>
        public IEnumerable<string> GetAllProps()
        {
            return m_props.Values.SelectMany(p => p).Distinct();
        }

        /// <summary>
        /// Returns a delegate that can be used for parsing of results for this module.
        /// Non-generic version of <see cref="QueryTypeProperties{T}.Parse"/>.
        /// </summary>
        public abstract Func<XElement, WikiInfo, object> Parser { get; }
    }

    /// <summary>
    /// Represents a module and its properties.
    /// </summary>
    public class QueryTypeProperties<T> : QueryTypeProperties
    {
        private readonly Func<XElement, WikiInfo, T> m_parser;

        public QueryTypeProperties(
            string moduleName, string prefix, QueryType? queryType, SortType? sortType,
            IEnumerable<Tuple<string, string>> baseParameters, IDictionary<string, string[]> props,
            Func<XElement, WikiInfo, T> parser)
            : base(moduleName, prefix, queryType, sortType, baseParameters, props)
        {
            m_parser = parser;
        }

        public QueryTypeProperties(
            string moduleName, string prefix, QueryType? queryType, SortType? sortType,
            IEnumerable<Tuple<string, string>> baseParameters, IDictionary<string, string[]> props,
            Func<XElement, T> parser)
            : this(moduleName, prefix, queryType, sortType, baseParameters, props, (elem, _) => parser(elem))
        {}

        /// <summary>
        /// Parses an XML element representing one item from the result.
        /// Returns the parsed object, possibly filled only partially,
        /// if some attributes of the XML element are missing.
        /// </summary>
        public T Parse(XElement element, WikiInfo wiki)
        {
            return m_parser(element, wiki);
        }

        public override Func<XElement, WikiInfo, object> Parser
        {
            get { return (element, info) => m_parser(element, info); }
        }
    }
}
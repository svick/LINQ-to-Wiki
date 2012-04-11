using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LinqToWiki
{
    /// <summary>
    /// Represents a certain type of query and its properties.
    /// </summary>
    public class QueryTypeProperties<T>
    {
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
        /// Parameters, that are included in all queries of this type.
        /// </summary>
        public IEnumerable<Tuple<string, string>> BaseParameters { get; private set; }

        private readonly IDictionary<string, string[]> m_props;
        private readonly Func<XElement, WikiInfo, T> m_parser;

        public QueryTypeProperties(
            string moduleName, string prefix, QueryType? queryType, IEnumerable<Tuple<string, string>> baseParameters,
            IDictionary<string, string[]> props, Func<XElement, WikiInfo, T> parser)
        {
            ModuleName = moduleName;
            Prefix = prefix;
            QueryType = queryType;
            BaseParameters = baseParameters;
            m_props = props ?? new Dictionary<string, string[]>();
            m_parser = parser;
        }

        public QueryTypeProperties(
            string moduleName, string prefix, QueryType? queryType, IEnumerable<Tuple<string, string>> baseParameters,
            IDictionary<string, string[]> props, Func<XElement, T> parser)
            : this(moduleName, prefix, queryType, baseParameters, props, (elem, _) => parser(elem))
        {}

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
        /// Parses an XML element representing one item from the result.
        /// Returns the parsed object, possibly filled only partially,
        /// if some attributes of the XML element are missing.
        /// </summary>
        public T Parse(XElement element, WikiInfo wiki)
        {
            return m_parser(element, wiki);
        }
    }
}
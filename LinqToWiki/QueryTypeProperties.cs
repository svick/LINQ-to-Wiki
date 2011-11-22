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
        /// Prefix that is used by this type of query.
        /// </summary>
        public string Prefix { get; private set; }

        /// <summary>
        /// Parameters, that are included in all queries of this type.
        /// </summary>
        public IEnumerable<Tuple<string, string>> BaseParameters { get; private set; }

        private readonly IDictionary<string, string> m_props;
        private readonly Func<XElement, T> m_parser;

        public QueryTypeProperties(
            string prefix, IEnumerable<Tuple<string, string>> baseParameters, IDictionary<string, string> props,
            Func<XElement, T> parser)
        {
            Prefix = prefix;
            BaseParameters = baseParameters;
            m_props = props;
            m_parser = parser;
        }

        /// <summary>
        /// Returns the value of <c>prop</c> for given property.
        /// </summary>
        public string GetProp(string property)
        {
            return m_props[property];
        }

        /// <summary>
        /// Returns all possible values for <c>prop</c>.
        /// </summary>
        public IEnumerable<string> GetAllProps()
        {
            return m_props.Values.Distinct();
        }

        /// <summary>
        /// Parses an XML element representing one item from the result.
        /// Returns the parsed object, possibly filled only partially,
        /// if some attributes of the XML element are missing.
        /// </summary>
        public T Parse(XElement element)
        {
            return m_parser(element);
        }
    }
}
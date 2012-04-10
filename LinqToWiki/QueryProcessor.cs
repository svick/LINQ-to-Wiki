using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Collections;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    /// <summary>
    /// Processes query parameters and parses the result of the query.
    /// </summary>
    public class QueryProcessor<T>
    {
        private readonly WikiInfo m_wiki;
        private readonly QueryTypeProperties<T> m_queryTypeProperties;

        public QueryProcessor(WikiInfo wiki, QueryTypeProperties<T> queryTypeProperties)
        {
            m_wiki = wiki;
            m_queryTypeProperties = queryTypeProperties;
        }

        /// <summary>
        /// Executes a query based on the <see cref="parameters"/>.
        /// </summary>
        public IEnumerable<TResult> Execute<TResult>(QueryParameters<T, TResult> parameters)
        {
            // TODO: too long, split

            var parsedParameters = new TupleList<string, string>(m_queryTypeProperties.BaseParameters);

            string prefix = m_queryTypeProperties.Prefix;

            if (parameters.Values != null)
                foreach (var value in parameters.Values)
                    parsedParameters.Add(prefix + value.Name, NameValuesParameter.JoinValues(value.Values));

            if (parameters.Ascending != null)
            {
                if (parameters.Sort != null)
                    parsedParameters.Add(prefix + "sort", parameters.Sort);

                // temporary! deal with asc/ascending in a systematic way
                parsedParameters.Add(prefix + "dir", parameters.Ascending.Value ? "ascending" : "descending");
            }

            var propList = new List<string>();

            if (parameters.Properties == null)
            {
                propList.AddRange(m_queryTypeProperties.GetAllProps());
            }
            else
            {
                foreach (var property in parameters.Properties)
                {
                    var prop = m_queryTypeProperties.GetProp(property);
                    if (prop != null && !propList.Contains(prop))
                        propList.Add(prop);
                }
            }

            parsedParameters.Add(prefix + "prop", NameValuesParameter.JoinValues(propList));

            parsedParameters.Add(prefix + "limit", "max");

            // TODO: add paging, maxlag

            var downloaded = m_wiki.Downloader.Download(parsedParameters);

            // TODO: handle errors

            // TODO: better way to access the elements
            return downloaded
                .Descendants(m_queryTypeProperties.ElementName)
                .Select(x => parameters.Selector(m_queryTypeProperties.Parse(x, m_wiki)));
        }
    }
}
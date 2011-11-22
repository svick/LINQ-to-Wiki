using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    /// <summary>
    /// Processes query parameters and parses the result of the query.
    /// </summary>
    public class QueryProcessor<T>
    {
        private readonly QueryTypeProperties<T> m_queryTypeProperties;
        private readonly Downloader m_downloader;

        public QueryProcessor(Wiki wiki, QueryTypeProperties<T> queryTypeProperties)
        {
            m_queryTypeProperties = queryTypeProperties;

            m_downloader = new Downloader(wiki);
        }

        /// <summary>
        /// Executes a query based on the <see cref="parameters"/>.
        /// </summary>
        public IEnumerable<TResult> Execute<TResult>(QueryParameters<T, TResult> parameters)
        {
            // TODO: too long, split

            var parsedParameters = new TupleList<string, string>(m_queryTypeProperties.BaseParameters);

            string prefix = m_queryTypeProperties.Prefix;

            foreach (var value in parameters.Values)
                parsedParameters.Add(prefix + value.Name, Join(value.Values));

            if (parameters.Sort != null)
            {
                parsedParameters.Add(prefix + "sort", parameters.Sort);

                if (parameters.Ascending != null)
                    parsedParameters.Add(prefix + "dir", parameters.Ascending.Value ? "asc" : "desc");
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

            parsedParameters.Add(prefix + "prop", Join(propList));

            // TODO: add paging

            var downloaded = m_downloader.Download(parsedParameters);

            return downloaded.Descendants(prefix).Select(x => parameters.Selector(m_queryTypeProperties.Parse(x)));
        }

        /// <summary>
        /// Formats a list of values into a form the API accepts.
        /// </summary>
        private static string Join(IEnumerable<string> values)
        {
            // TODO: escaping?

            return string.Join("|", values);
        }
    }
}
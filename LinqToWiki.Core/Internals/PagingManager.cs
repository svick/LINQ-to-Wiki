using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LinqToWiki.Download;
using LinqToWiki.Parameters;

namespace LinqToWiki.Internals
{
    /// <summary>
    /// Handles secondary paging for a set of pages from the same primary page.
    /// </summary>
    class PagingManager
    {
        private readonly WikiInfo m_wiki;
        private readonly string m_generator;
        private readonly IEnumerable<PropQueryParameters> m_propQueryParametersCollection;
        private readonly IEnumerable<HttpQueryParameterBase> m_currentParameters;
        private readonly Dictionary<string, QueryTypeProperties> m_pageProperties;
        private readonly HttpQueryParameter m_primaryQueryContinue;
        private Dictionary<string, HttpQueryParameter> m_secondaryQueryContinues;
        private Dictionary<long, PageData> m_pages;

        public PagingManager(
            WikiInfo wiki, string generator, IEnumerable<PropQueryParameters> propQueryParametersCollection,
            IEnumerable<HttpQueryParameterBase> currentParameters, Dictionary<string, QueryTypeProperties> pageProperties,
            HttpQueryParameter primaryQueryContinue,
            Dictionary<string, HttpQueryParameter> secondaryQueryContinues)
        {
            m_wiki = wiki;
            m_generator = generator;
            m_propQueryParametersCollection = propQueryParametersCollection;
            m_currentParameters = currentParameters;
            m_pageProperties = pageProperties;
            m_primaryQueryContinue = primaryQueryContinue;
            m_secondaryQueryContinues = secondaryQueryContinues;
        }

        /// <summary>
        /// Sets the set of pages to handle.
        /// </summary>
        public void SetPages(IEnumerable<PageData> pages)
        {
            m_pages = pages.Where(p => p.PageId != null).ToDictionary(p => p.PageId.Value);
        }

        /// <summary>
        /// Are there any more secondary pages for the given property?
        /// </summary>
        public bool HasMore(string name)
        {
            return m_secondaryQueryContinues.ContainsKey(name);
        }

        /// <summary>
        /// Retrieves one more secondary page
        /// and puts data from it to the corresponding <see cref="PageData"/>.
        /// </summary>
        public void GetMore()
        {
            var downloaded = QueryProcessor.Download(
                m_wiki,
                QueryPageProcessor.ProcessParameters(
                    m_propQueryParametersCollection, m_currentParameters,
                    m_pageProperties, false, m_secondaryQueryContinues.Keys),
                new[] { m_primaryQueryContinue }.Concat(m_secondaryQueryContinues.Values));

            var queryContinues = QueryProcessor.GetQueryContinues(downloaded);

            if (m_generator != null)
                queryContinues.Remove(m_generator);

            var pageElements = downloaded.Element("query").Element("pages").Elements("page");

            foreach (var pageElement in pageElements)
            {
                long pageId = ValueParser.ParseInt64(pageElement.Attribute("pageid").Value);

                PageData pageData;
                if (m_pages.TryGetValue(pageId, out pageData))
                {
                    Contract.Assume(pageData != null);

                    pageData.AddData(pageElement);
                }
            }

            m_secondaryQueryContinues = queryContinues;
        }
    }
}
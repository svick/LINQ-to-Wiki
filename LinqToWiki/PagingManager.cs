using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    class PagingManager
    {
        private readonly WikiInfo m_wiki;
        private readonly string m_generator;
        private readonly PageQueryParameters m_parameters;
        private readonly Dictionary<string, QueryTypeProperties> m_pageProperties;
        private readonly Tuple<string, string> m_primaryQueryContinue;
        private Dictionary<string, Tuple<string, string>> m_secondaryQueryContinues;
        private Dictionary<long, PageData> m_pages;

        public PagingManager(
            WikiInfo wiki, string generator, PageQueryParameters parameters,
            Dictionary<string, QueryTypeProperties> pageProperties, Tuple<string, string> primaryQueryContinue,
            Dictionary<string, Tuple<string, string>> secondaryQueryContinues)
        {
            m_wiki = wiki;
            m_generator = generator;
            m_parameters = parameters;
            m_pageProperties = pageProperties;
            m_primaryQueryContinue = primaryQueryContinue;
            m_secondaryQueryContinues = secondaryQueryContinues;
        }

        public void SetPages(IEnumerable<PageData> pages)
        {
            m_pages = pages.ToDictionary(p => p.PageId);
        }

        public bool HasMore(string name)
        {
            return m_secondaryQueryContinues.ContainsKey(name);
        }

        public void GetMore()
        {
            var downloaded = QueryProcessor.Download(
                m_wiki, QueryPageProcessor.ProcessParameters(m_parameters, m_pageProperties, false),
                new[] { m_primaryQueryContinue }.Concat(m_secondaryQueryContinues.Values));

            var queryContinues = QueryProcessor.GetQueryContinues(downloaded);

            queryContinues.Remove(m_generator);

            var pageElements = downloaded.Element("query").Element("pages").Elements("page");

            foreach (var pageElement in pageElements)
            {
                long pageId = ValueParser.ParseInt64(pageElement.Attribute("pageid").Value);

                PageData pageData;
                if (m_pages.TryGetValue(pageId, out pageData))
                    pageData.AddData(pageElement);
            }

            m_secondaryQueryContinues = queryContinues;
        }
    }
}
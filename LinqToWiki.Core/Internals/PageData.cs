using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LinqToWiki.Internals
{
    /// <summary>
    /// Represents information about a single page from a page source query,
    /// including information from secondary paging.
    /// </summary>
    class PageData
    {
        private readonly WikiInfo m_wiki;
        private readonly Dictionary<string, QueryTypeProperties> m_pageProperties;
        private readonly PagingManager m_pagingManager;

        /// <summary>
        /// Contains named data lists.
        /// Each data list contains data for given property (e.g. <c>categories</c>) from already retrieved
        /// secondary paging.
        /// </summary>
        private readonly Dictionary<string, List<object>> m_data = new Dictionary<string, List<object>>();

        public PageData(
            WikiInfo wiki, XElement element, Dictionary<string, QueryTypeProperties> pageProperties,
            PagingManager pagingManager)
        {
            m_wiki = wiki;
            m_pageProperties = pageProperties;
            m_pagingManager = pagingManager;

            GetOrCreateDataList("info").Add(m_pageProperties["info"].Parser(element, wiki));

            var pageIdAttribute = element.Attribute("pageid");

            if (pageIdAttribute != null)
            {
                PageId = ValueParser.ParseInt64(pageIdAttribute.Value);

                AddData(element);
            }
        }

        /// <summary>
        /// Page ID of the page.
        /// Can be <c>null</c> for missing or invalid pages.
        /// </summary>
        internal long? PageId { get; private set; }

        /// <summary>
        /// When secondary paging progresses, this method is used to add new data
        /// to the <see cref="PageData"/> object.
        /// </summary>
        internal void AddData(XElement element)
        {
            foreach (var dataElement in element.Elements())
            {
                string name = dataElement.Name.LocalName;

                var parser = m_pageProperties[name].Parser;

                var dataList = GetOrCreateDataList(name);

                if (dataElement.HasAttributes)
                    dataList.Add(parser(dataElement, m_wiki));
                else
                    dataList.AddRange(dataElement.Elements().Select(e => parser(e, m_wiki)));
            }
        }

        /// <summary>
        /// Returns a data list with the given name.
        /// If one doesn't exist yet, it's created first.
        /// </summary>
        private List<object> GetOrCreateDataList(string name)
        {
            List<object> dataList;
            if (!m_data.TryGetValue(name, out dataList))
            {
                dataList = new List<object>();
                m_data.Add(name, dataList);
            }
            return dataList;
        }

        /// <summary>
        /// Returns the collection of objects for the prop with the given name
        /// for this page.
        /// The collection is lazy and causes secondary paging to progress if necessary.
        /// </summary>
        public IEnumerable<T> GetData<T>(string name)
        {
            var dataList = GetOrCreateDataList(name);

            int i = 0;

            while (true)
            {
                while (dataList.Count <= i && m_pagingManager.HasMore(name))
                    m_pagingManager.GetMore();

                if (dataList.Count <= i)
                    break;

                yield return (T)dataList[i++];
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LinqToWiki
{
    class PageData
    {
        private readonly WikiInfo m_wiki;
        private readonly Dictionary<string, QueryTypeProperties> m_pageProperties;
        private readonly PagingManager m_pagingManager;

        private readonly object m_info;
        private readonly Dictionary<string, List<object>> m_data = new Dictionary<string, List<object>>();

        public PageData(
            WikiInfo wiki, XElement element, Dictionary<string, QueryTypeProperties> pageProperties,
            PagingManager pagingManager)
        {
            m_wiki = wiki;
            m_pageProperties = pageProperties;
            m_pagingManager = pagingManager;

            m_info = m_pageProperties["info"].Parser(element, wiki);

            PageId = ValueParser.ParseInt64(element.Attribute("pageid").Value);

            AddData(element);
        }

        internal long PageId { get; private set; }

        internal void AddData(XElement element)
        {
            foreach (var dataElement in element.Elements())
            {
                string name = dataElement.Name.LocalName;

                var parser = m_pageProperties[name].Parser;

                var dataList = GetOrCreateDataList(name);

                dataList.AddRange(dataElement.Elements().Select(e => parser(e, m_wiki)));
            }
        }

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

        public TInfo GetInfo<TInfo>()
        {
            return (TInfo)m_info;
        }

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
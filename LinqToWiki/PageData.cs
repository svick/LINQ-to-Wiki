using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LinqToWiki
{
    public class PageData
    {
        private readonly WikiInfo m_wiki;
        private readonly XElement m_element;
        private readonly Dictionary<string, QueryTypeProperties> m_pageProperties;

        public PageData(WikiInfo wiki, XElement element, Dictionary<string, QueryTypeProperties> pageProperties)
        {
            m_wiki = wiki;
            m_element = element;
            m_pageProperties = pageProperties;
        }

        public TInfo GetInfo<TInfo>()
        {
            return (TInfo)m_pageProperties["info"].Parser(m_element, m_wiki);
        }

        public IEnumerable<T> GetData<T>(string name)
        {
            var dataElement = m_element.Element(name);

            if (dataElement == null)
                return new T[0];

            var parser = m_pageProperties[name].Parser;

            return dataElement.Elements().Select(e => (T)parser(e, m_wiki));
        }
    }
}
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

        public PageData(WikiInfo wiki, XElement element)
        {
            m_wiki = wiki;
            m_element = element;
        }

        public TInfo GetInfo<TInfo>(Func<XElement, WikiInfo, TInfo> parser)
        {
            return parser(m_element, m_wiki);
        }

        public IEnumerable<T> GetData<T>(string name, Func<XElement, WikiInfo, T> parser)
        {
            var dataElement = m_element.Element(name);

            if (dataElement == null)
                return new T[0];

            return dataElement.Elements().Select(e => parser(e, m_wiki));
        }
    }
}
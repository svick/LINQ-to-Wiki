using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace LinqToWiki
{
    /// <summary>
    /// Downloads the results of a query from the wiki web site.
    /// </summary>
    public class Downloader
    {
        private readonly Wiki m_wiki;

        public Downloader(Wiki wiki)
        {
            m_wiki = wiki;
        }

        /// <summary>
        /// Downloads the results of query defined by <see cref="parameters"/>.
        /// </summary>
        public XDocument Download(IEnumerable<Tuple<string, string>> parameters)
        {
            throw new NotImplementedException();
        }
    }
}
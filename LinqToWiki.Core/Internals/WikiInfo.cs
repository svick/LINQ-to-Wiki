using System;
using System.Collections.Generic;

namespace LinqToWiki.Internals
{
    /// <summary>
    /// Contains information about a wiki necessary to access its API.
    /// </summary>
    public class WikiInfo
    {
        /// <summary>
        /// Base URL of the wiki.
        /// For example <c>http://en.wikipedia.org/</c> or <c>http://localhost/wiki/</c>.
        /// </summary>
        public Uri BaseUrl { get; private set; }

        /// <summary>
        /// The absolute URL of api.php.
        /// For example <c>http://en.wikipedia.org/w/api.php</c> or <c>http://localhost/wiki/api.php</c>.
        /// </summary>
        public Uri ApiUrl { get; private set; }

        /// <summary>
        /// Object that can be used to execute queries against this wiki.
        /// </summary>
        public Downloader Downloader { get; private set; }

        /// <summary>
        /// Collection of namespaces of this wiki.
        /// </summary>
        public NamespaceInfo Namespaces { get; private set; }

        public WikiInfo(string baseUrl = null, string apiPath = null, IEnumerable<Namespace> namespaces = null)
        {
            if (baseUrl == null)
                baseUrl = "en.wikipedia.org";

            if (!baseUrl.StartsWith("http"))
                baseUrl = "http://" + baseUrl;

            BaseUrl = new Uri(baseUrl);

            if (apiPath == null)
                apiPath = "/w/api.php";

            ApiUrl = new Uri(BaseUrl, apiPath);

            Downloader = new Downloader(this);

            if (namespaces == null)
                Namespaces = new NamespaceInfo(this);
            else
                Namespaces = new NamespaceInfo(namespaces);
        }
    }
}
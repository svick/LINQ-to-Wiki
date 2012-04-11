using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Collections;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    /// <summary>
    /// Contains information about a certain wiki necessary to access its API.
    /// </summary>
    public class WikiInfo
    {
        public Uri BaseUrl { get; private set; }

        public Uri ApiUrl { get; private set; }

        public Downloader Downloader { get; private set; }

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

    public class NamespaceInfo : IEnumerable<Namespace>
    {
        private readonly Dictionary<int, Namespace> m_namespaces;

        internal NamespaceInfo(IEnumerable<Namespace> namespaces)
        {
            m_namespaces = namespaces.ToDictionary(ns => ns.Id);
        }

        public NamespaceInfo(WikiInfo wiki)
            : this(GetNamespaces(wiki))
        {}

        private static IEnumerable<Namespace> GetNamespaces(WikiInfo wiki)
        {
            var queryProcessor = new QueryProcessor<IEnumerable<Namespace>>(
                wiki,
                new QueryTypeProperties<IEnumerable<Namespace>>(
                    "siteinfo", "", QueryType.Meta,
                    new TupleList<string, string>
                    { { "action", "query" }, { "meta", "siteinfo" }, { "siprop", "namespaces" } },
                    null, Namespace.Parse));

            return queryProcessor.ExecuteSingle(QueryParameters.Create<IEnumerable<Namespace>>());
        }

        public Namespace this[int id]
        {
            get { return m_namespaces[id]; }
        }

        public IEnumerator<Namespace> GetEnumerator()
        {
            return m_namespaces.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
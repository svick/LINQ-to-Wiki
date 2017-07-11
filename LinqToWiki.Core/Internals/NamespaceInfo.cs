using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Collections;
using LinqToWiki.Parameters;

namespace LinqToWiki.Internals
{
    /// <summary>
    /// Contains information about namespaces in a wiki.
    /// </summary>
    public class NamespaceInfo : IEnumerable<Namespace>
    {
        private readonly Lazy<Dictionary<int, Namespace>> m_namespaces;

        /// <summary>
        /// Creates a <see cref="NamespaceInfo"/> based on a function to get a collection of namespaces.
        /// </summary>
        /// <param name="namespaces"></param>
        private NamespaceInfo(Func<IEnumerable<Namespace>> namespaces)
        {
            m_namespaces = new Lazy<Dictionary<int, Namespace>>(
                () => namespaces().ToDictionary(ns => ns.Id));
        }
        /// <summary>
        /// Creates a <see cref="NamespaceInfo"/> based on a collection of namespaces.
        /// </summary>
        /// <param name="namespaces"></param>
        internal NamespaceInfo(IEnumerable<Namespace> namespaces)
            : this(() => namespaces)
        {
        }

        /// <summary>
        /// Creates a <see cref="NamespaceInfo"/> by downloading the namespace information
        /// for a wiki.
        /// </summary>
        public NamespaceInfo(WikiInfo wiki)
            : this(() => GetNamespaces(wiki))
        {
        }

        /// <summary>
        /// Downloads the namespaces for a wiki.
        /// </summary>
        private static IEnumerable<Namespace> GetNamespaces(WikiInfo wiki)
        {
            var queryProcessor = new QueryProcessor<IEnumerable<Namespace>>(
                wiki,
                new QueryTypeProperties<IEnumerable<Namespace>>(
                    "siteinfo", "", QueryType.Meta, null,
                    new TupleList<string, string>
                    { { "action", "query" }, { "meta", "siteinfo" }, { "siprop", "namespaces" } },
                    null, Namespace.Parse));

            return queryProcessor.ExecuteSingle(QueryParameters.Create<IEnumerable<Namespace>>());
        }

        /// <summary>
        /// Gets namespace by its ID.
        /// </summary>
        public Namespace this[int id]
        {
            get { return m_namespaces.Value[id]; }
        }

        public IEnumerator<Namespace> GetEnumerator()
        {
            return m_namespaces.Value.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
using System;
using System.Collections.Generic;
using LinqToWiki.Collections;
using LinqToWiki.Download;
using LinqToWiki.Internals;

namespace LinqToWiki
{
    /// <summary>
    /// Collection of pages for a <see cref="ListSourceBase{TPage}"/>.
    /// </summary>
    class ListPagesCollection : IPagesCollection
    {
        private readonly string m_parameterName;
        private readonly int m_pageSize;
        private readonly IEnumerator<string> m_enumerator;
        private bool m_iterating;
        private bool m_shouldMove = true;

        public ListPagesCollection(string parameterName, IEnumerable<string> values, int pageSize)
        {
            m_parameterName = parameterName;
            m_pageSize = pageSize;
            m_enumerator = values.GetEnumerator();
        }

        public bool HasMorePages(HttpQueryParameter primaryQueryContinue)
        {
            MoveIfNecessary();
            return m_iterating;
        }

        /// <summary>
        /// Moves the internal enumerator if necessary.
        /// </summary>
        private void MoveIfNecessary()
        {
            if (m_shouldMove)
            {
                m_iterating = m_enumerator.MoveNext();
                m_shouldMove = false;
            }
        }

        public IEnumerable<HttpQueryParameterBase> GetNextPage(int limit)
        {
            if (limit == -1)
                limit = m_pageSize;

            var formattedValues = new List<string>();
            int i = 0;

            MoveIfNecessary();

            while (m_iterating)
            {
                formattedValues.Add(m_enumerator.Current);
                m_shouldMove = true;
                if (++i >= limit)
                    break;
                MoveIfNecessary();
            }
            return new[] { new HttpQueryParameter(m_parameterName, formattedValues.ToQueryString()) };
        }
    }
}
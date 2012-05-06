using System;
using System.Collections.Generic;
using LinqToWiki.Collections;
using LinqToWiki.Internals;

namespace LinqToWiki
{
    /// <summary>
    /// Collection of pages for a <see cref="ListSourceBase{TPage}"/>.
    /// </summary>
    class ListPagesCollection : IPagesCollection
    {
        static ListPagesCollection()
        {
            MaxLimit = 50;
        }

        /// <summary>
        /// How many pages should be in a single page. Defaults to 50.
        /// Won't work correctly if set to more than the maximum set by the wiki.
        /// </summary>
        public static int MaxLimit { get; set; }

        private readonly string m_parameterName;
        private readonly IEnumerator<string> m_enumerator;
        private bool m_iterating;
        private bool m_shouldMove = true;

        public ListPagesCollection(string parameterName, IEnumerable<string> values)
        {
            m_parameterName = parameterName;
            m_enumerator = values.GetEnumerator();
        }

        public bool HasMorePages(Tuple<string, string> primaryQueryContinue)
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

        public IEnumerable<Tuple<string, string>> GetNextPage(int limit)
        {
            if (limit == -1)
                limit = MaxLimit;

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
            return new TupleList<string, string> { { m_parameterName, formattedValues.ToQueryString() } };
        }
    }
}
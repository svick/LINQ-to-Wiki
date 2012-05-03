using System;
using System.Collections.Generic;
using LinqToWiki.Collections;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    class ListPagesCollection : IPagesCollection
    {
        static ListPagesCollection()
        {
            MaxLimit = 50;
        }

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
            return new TupleList<string, string> { { m_parameterName, NameValueParameter.JoinValues(formattedValues) } };
        }
    }
}
using System;
using System.Collections.Generic;
using LinqToWiki.Collections;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    class ListPagesCollection : IPagesCollection
    {
        private readonly string m_parameterName;
        private readonly IEnumerator<string> m_enumerator;
        private bool m_iterating;

        public ListPagesCollection(string parameterName, IEnumerable<string> values)
        {
            m_parameterName = parameterName;
            m_enumerator = values.GetEnumerator();
            m_iterating = m_enumerator.MoveNext();
        }

        public bool HasMorePages(Tuple<string, string> primaryQueryContinue)
        {
            return m_iterating;
        }

        public IEnumerable<Tuple<string, string>> GetNextPage(int limit)
        {
            var formattedValues = new List<string>();
            int i = 0;
            while (m_iterating)
            {
                formattedValues.Add(m_enumerator.Current);
                m_iterating = m_enumerator.MoveNext();
                if (limit != -1 && ++i >= limit)
                    break;
            }
            return new TupleList<string, string> { { m_parameterName, NameValueParameter.JoinValues(formattedValues) } };
        }
    }
}
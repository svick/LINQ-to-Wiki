using System;
using System.Collections.Generic;

namespace LinqToWiki
{
    class GeneratorPagesCollection : IPagesCollection
    {
        private readonly Func<int, IEnumerable<Tuple<string, string>>> m_baseParametersSelector;

        public GeneratorPagesCollection(Func<int, IEnumerable<Tuple<string, string>>> baseParametersSelector)
        {
            m_baseParametersSelector = baseParametersSelector;
        }

        public bool HasMorePages(Tuple<string, string> primaryQueryContinue)
        {
            return primaryQueryContinue != null;
        }

        public IEnumerable<Tuple<string, string>> GetNextPage(int limit)
        {
            return m_baseParametersSelector(limit);
        }
    }
}
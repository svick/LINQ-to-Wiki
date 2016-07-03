using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LinqToWiki.Download;

namespace LinqToWiki
{
    /// <summary>
    /// Collection of pages for a <see cref="GeneratorPagesSource{TPage}"/>.
    /// </summary>
    class GeneratorPagesCollection : IPagesCollection
    {
        private readonly Func<int, IEnumerable<HttpQueryParameterBase>> m_baseParametersSelector;

        public GeneratorPagesCollection(Func<int, IEnumerable<HttpQueryParameterBase>> baseParametersSelector)
        {
            m_baseParametersSelector = baseParametersSelector;
        }

        public bool HasMorePages(HttpQueryParameter primaryQueryContinue)
        {
            return primaryQueryContinue != null;
        }

        public IEnumerable<HttpQueryParameterBase> GetNextPage(int limit)
        {
            return m_baseParametersSelector(limit);
        }

        [ContractInvariantMethod]
        private void Invariants()
        {
            Contract.Invariant(m_baseParametersSelector != null);
        }
    }
}
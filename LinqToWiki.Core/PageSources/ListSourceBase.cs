using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LinqToWiki.Internals;

namespace LinqToWiki
{
    /// <summary>
    /// Base type for page sources based on a static list of pages.
    /// </summary>
    public abstract class ListSourceBase<TPage> : PagesSource<TPage>
    {
        private readonly string m_parameterName;
        private readonly IEnumerable<string> m_values;
        private readonly int m_pageSize;

        protected ListSourceBase(WikiInfo wiki, string parameterName, IEnumerable<string> values)
            : base(new QueryPageProcessor(wiki))
        {
            Contract.Requires(wiki != null);

            m_parameterName = parameterName;
            m_values = values;
            m_pageSize = wiki.PagesSourcePageSize;
        }

        protected override IPagesCollection GetPagesCollection()
        {
            return new ListPagesCollection(m_parameterName, m_values, m_pageSize);
        }

        [ContractInvariantMethod]
        private void Invariants()
        {
            Contract.Invariant(m_values != null);
        }
    }
}
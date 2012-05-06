using System.Collections.Generic;
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

        protected ListSourceBase(WikiInfo wiki, string parameterName, IEnumerable<string> values)
            : base(new QueryPageProcessor(wiki))
        {
            m_parameterName = parameterName;
            m_values = values;
        }

        protected override IPagesCollection GetPagesCollection()
        {
            return new ListPagesCollection(m_parameterName, m_values);
        }
    }
}
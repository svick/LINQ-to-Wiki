using System.Collections.Generic;
using LinqToWiki.Internals;

namespace LinqToWiki
{
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

        protected override IPagesCollection GetPagesCollectionInternal()
        {
            return new ListPagesCollection(m_parameterName, m_values);
        }
    }
}
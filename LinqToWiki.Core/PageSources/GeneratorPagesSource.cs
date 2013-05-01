using System;
using System.Collections.Generic;
using LinqToWiki.Download;
using LinqToWiki.Internals;

namespace LinqToWiki
{
    /// <summary>
    /// Page source for a generator.
    /// </summary>
    public sealed class GeneratorPagesSource<TPage> : PagesSource<TPage>
    {
        private readonly Func<int, IEnumerable<HttpQueryParameterBase>> m_baseParametersSelector;

        public GeneratorPagesSource(
            Func<int, IEnumerable<HttpQueryParameterBase>> baseParametersSelector, QueryPageProcessor queryPageProcessor)
            : base(queryPageProcessor)
        {
            m_baseParametersSelector = baseParametersSelector;
        }

        protected override IPagesCollection GetPagesCollection()
        {
            return new GeneratorPagesCollection(m_baseParametersSelector);
        }
    }
}
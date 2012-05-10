using System;
using System.Collections.Generic;
using LinqToWiki.Internals;

namespace LinqToWiki
{
    /// <summary>
    /// Page source for a generator.
    /// </summary>
    public sealed class GeneratorPagesSource<TPage> : PagesSource<TPage>
    {
        private readonly Func<int, IEnumerable<Tuple<string, string>>> m_baseParametersSelector;

        public GeneratorPagesSource(
            Func<int, IEnumerable<Tuple<string, string>>> baseParametersSelector, QueryPageProcessor queryPageProcessor)
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
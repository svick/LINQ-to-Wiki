using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LinqToWiki.Internals;

namespace LinqToWiki
{
    /// <summary>
    /// Page source based on a static collection of page titles.
    /// </summary>
    public class TitlesSource<TPage> : ListSourceBase<TPage>
    {
        public TitlesSource(WikiInfo wiki, IEnumerable<string> titles)
            : base(wiki, "titles", titles)
        {
            Contract.Requires(wiki != null);
        }
    }
}
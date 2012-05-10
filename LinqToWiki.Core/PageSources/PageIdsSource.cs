using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Internals;

namespace LinqToWiki
{
    /// <summary>
    /// Page source based on a static collection of page IDs.
    /// </summary>
    public class PageIdsSource<TPage> : ListSourceBase<TPage>
    {
        public PageIdsSource(WikiInfo wiki, IEnumerable<long> pageIds)
            : base(wiki, "pageids", pageIds.Select(id => id.ToQueryString()))
        {}
    }
}
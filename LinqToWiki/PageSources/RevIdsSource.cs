using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Internals;

namespace LinqToWiki
{
    /// <summary>
    /// Page source based on a static collection of revision IDs.
    /// </summary>
    public class RevIdsSource<TPage> : ListSourceBase<TPage>
    {
        public RevIdsSource(WikiInfo wiki, IEnumerable<long> revIds)
            : base(wiki, "revids", revIds.Select(id => id.ToQueryString()))
        {}
    }
}
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LinqToWiki.Internals;

namespace LinqToWiki
{
    public class PageIdsSource<TPage> : ListSourceBase<TPage>
    {
        public PageIdsSource(WikiInfo wiki, IEnumerable<long> pageIds)
            : base(wiki, "pageids", pageIds.Select(id => id.ToString(CultureInfo.InvariantCulture)))
        {}
    }
}
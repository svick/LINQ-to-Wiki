using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LinqToWiki.Internals;

namespace LinqToWiki
{
    public class RevIdsSource<TPage> : ListSourceBase<TPage>
    {
        public RevIdsSource(WikiInfo wiki, IEnumerable<long> revIds)
            : base(wiki, "revids", revIds.Select(id => id.ToString(CultureInfo.InvariantCulture)))
        {}
    }
}
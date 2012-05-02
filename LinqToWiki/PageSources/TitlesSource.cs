using System.Collections.Generic;
using LinqToWiki.Internals;

namespace LinqToWiki
{
    public class TitlesSource<TPage> : ListSourceBase<TPage>
    {
        public TitlesSource(WikiInfo wiki, IEnumerable<string> titles)
            : base(wiki, "titles", titles)
        {}
    }
}
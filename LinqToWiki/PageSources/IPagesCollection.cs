using System;
using System.Collections.Generic;

namespace LinqToWiki
{
    public interface IPagesCollection
    {
        bool HasMorePages(Tuple<string, string> primaryQueryContinue);
        IEnumerable<Tuple<string, string>> GetNextPage(int limit);
    }
}
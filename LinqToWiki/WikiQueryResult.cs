using System.Collections.Generic;
using System.Linq;

namespace LinqToWiki
{
    public class WikiQueryResult<T>
    {
        public List<T> ToList()
        {
            return new List<T>();
        }

        public IEnumerable<T> ToEnumerable()
        {
            return Enumerable.Empty<T>();
        }
    }
}
using System;
using System.Collections.Generic;

namespace LinqToWiki
{
    /// <summary>
    /// Represents a set of pages, can be used to retrieve query parameters
    /// for batches of the set.
    /// Similar to <see cref="System.Collections.Generic.IEnumerator{T}"/>,
    /// when compared with <see cref="PagesSource{TPage}"/>.
    /// </summary>
    public interface IPagesCollection
    {
        /// <summary>
        /// Whether there are any more pages of pages available.
        /// </summary>
        bool HasMorePages(Tuple<string, string> primaryQueryContinue);

        /// <summary>
        /// Returns the parameters to retrieve the next page of pages.
        /// </summary>
        IEnumerable<Tuple<string, string>> GetNextPage(int limit);
    }
}
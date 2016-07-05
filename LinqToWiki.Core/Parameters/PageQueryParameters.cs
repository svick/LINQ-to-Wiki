using System;
using System.Collections.Generic;

namespace LinqToWiki.Parameters
{
    /// <summary>
    /// Parameters for a page source query.
    /// </summary>
    public class PageQueryParameters
    {
        public PageQueryParameters(IPagesCollection pagesCollection)
        {
            if (pagesCollection == null)
                throw new ArgumentNullException(nameof(pagesCollection));

            PagesCollection = pagesCollection;
        }

        /// <summary>
        /// Collection of primary pages.
        /// </summary>
        public IPagesCollection PagesCollection { get; private set; }

        /// <summary>
        /// Parameters for each prop.
        /// </summary>
        public IEnumerable<PropQueryParameters> PropQueryParametersCollection { get; private set; }

        /// <summary>
        /// Returns new <see cref="PageQueryParameters"/> that is a copy of this instance
        /// with <see cref="PropQueryParametersCollection"/> set.
        /// </summary>
        /// <param name="parametersCollection"></param>
        /// <returns></returns>
        public PageQueryParameters WithParameters(IEnumerable<PropQueryParameters> parametersCollection)
        {
            if (PropQueryParametersCollection != null)
                throw new InvalidOperationException();

            return new PageQueryParameters(PagesCollection) { PropQueryParametersCollection = parametersCollection };
        }
    }
}
using System;

namespace LinqToWiki
{
    /// <summary>
    /// Marker interface for prop modules that show only the first item for each page
    /// (i.e. <c>revisions</c>).
    /// </summary>
    public interface IFirst
    {}

    /// <summary>
    /// Extensions for modules that implement <see cref="IFirst"/>.
    /// </summary>
    public static class WikiQueryFirstExtension
    {
        /// <summary>
        /// Selects the first result for each page, or <c>null</c>, if there are no results.
        /// </summary>
        public static TResult FirstOrDefault<TSource, TResult>(this WikiQueryResult<TSource, TResult> wikiQuery)
            where TSource : IFirst
        {
            throw new NotSupportedException();
        }
    }
}
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    /// <summary>
    /// Non-generic base interface for <see cref="WikiQueryResult{TSource,TResult}"/>.
    /// </summary>
    interface IWikiQueryResult
    {
        /// <summary>
        /// Parameters that can be used to execute the query.
        /// </summary>
        QueryParameters Parameters { get; }
    }
}
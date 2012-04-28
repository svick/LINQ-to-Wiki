using LinqToWiki.Parameters;

namespace LinqToWiki
{
    interface IWikiQueryResult
    {
        QueryParameters Parameters { get; }
    }
}
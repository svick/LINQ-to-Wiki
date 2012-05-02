using LinqToWiki.Internals;

namespace LinqToWiki
{
    interface IPagesSource
    {
        IPagesCollection GetPagesCollection();
        QueryPageProcessor QueryPageProcessor { get; }
    }
}
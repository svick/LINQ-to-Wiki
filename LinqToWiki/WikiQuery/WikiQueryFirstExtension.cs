using System;

namespace LinqToWiki
{
    public interface IFirst
    {}

    public static class WikiQueryFirstExtension
    {
        public static TResult FirstOrDefault<TSource, TResult>(this WikiQueryResult<TSource, TResult> wikiQuery)
            where TSource : IFirst
        {
            throw new NotSupportedException();
        }
    }
}
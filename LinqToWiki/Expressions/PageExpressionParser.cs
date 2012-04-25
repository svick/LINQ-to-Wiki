using System;
using System.Linq.Expressions;
using LinqToWiki.Parameters;

namespace LinqToWiki.Expressions
{
    public static class PageExpressionParser
    {
         public static PageQueryParameters ParseSelect<TSource, TResult>(
            ref Expression<Func<TSource, TResult>> expression, PageQueryParameters baseParameters)
         {
             throw new NotImplementedException();
         }
    }
}
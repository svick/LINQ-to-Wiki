using LinqToWiki.Internals;
using System.Linq.Expressions;
using LinqToWiki.Parameters;
// <copyright file="PageExpressionParserTest.cs">Copyright ©  2011</copyright>

using System;
using LinqToWiki.Expressions;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToWiki.Expressions.Tests
{
    [TestClass]
    [PexClass(typeof(PageExpressionParser))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class PageExpressionParserTest
    {

        [PexGenericArguments(typeof(int), typeof(int))]
        [PexMethod]
        internal PageQueryParameters ParseSelect<TSource, TResult>(
            Expression<Func<TSource, TResult>> expression,
            PageQueryParameters baseParameters,
            out Func<PageData, TResult> processedExpression
        )
        {
            PageQueryParameters result = PageExpressionParser.ParseSelect<TSource, TResult>
                                             (expression, baseParameters, out processedExpression);
            return result;
            // TODO: add assertions to method PageExpressionParserTest.ParseSelect(Expression`1<Func`2<!!0,!!1>>, PageQueryParameters, Func`2<PageData,!!1>&)
        }
    }
}

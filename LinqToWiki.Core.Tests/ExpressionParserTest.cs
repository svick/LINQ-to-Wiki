using System.Linq.Expressions;
using LinqToWiki.Parameters;
// <copyright file="ExpressionParserTest.cs">Copyright ©  2011</copyright>

using System;
using LinqToWiki.Expressions;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToWiki.Expressions.Tests
{
    [TestClass]
    [PexClass(typeof(ExpressionParser))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class ExpressionParserTest
    {

        [PexGenericArguments(typeof(int))]
        [PexMethod]
        internal QueryParameters<T, T> ParseIdentitySelect<T>(Expression<Func<T, T>> expression, QueryParameters<T, T> previousParameters)
        {
            QueryParameters<T, T> result
               = ExpressionParser.ParseIdentitySelect<T>(expression, previousParameters);
            return result;
            // TODO: add assertions to method ExpressionParserTest.ParseIdentitySelect(Expression`1<Func`2<!!0,!!0>>, QueryParameters`2<!!0,!!0>)
        }

        [PexGenericArguments(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) })]
        [PexMethod]
        internal QueryParameters<TSource, TResult> ParseOrderBy<TSource, TResult, TOrderBy, TKey>(
            Expression<Func<TOrderBy, TKey>> expression,
            QueryParameters<TSource, TResult> previousParameters,
            bool ascending
        )
        {
            QueryParameters<TSource, TResult> result
               = ExpressionParser.ParseOrderBy<TSource, TResult, TOrderBy, TKey>
                     (expression, previousParameters, ascending);
            return result;
            // TODO: add assertions to method ExpressionParserTest.ParseOrderBy(Expression`1<Func`2<!!2,!!3>>, QueryParameters`2<!!0,!!1>, Boolean)
        }

        [PexGenericArguments(typeof(int), typeof(int))]
        [PexMethod]
        internal QueryParameters<TSource, TResult> ParseSelect<TSource, TResult>(Expression<Func<TSource, TResult>> expression, QueryParameters<TSource, TSource> previousParameters)
        {
            QueryParameters<TSource, TResult> result
               = ExpressionParser.ParseSelect<TSource, TResult>(expression, previousParameters);
            return result;
            // TODO: add assertions to method ExpressionParserTest.ParseSelect(Expression`1<Func`2<!!0,!!1>>, QueryParameters`2<!!0,!!0>)
        }

        [PexGenericArguments(typeof(int), typeof(int), typeof(int))]
        [PexMethod]
        internal QueryParameters<TSource, TResult> ParseWhere<TSource, TResult, TWhere>(Expression<Func<TWhere, bool>> expression, QueryParameters<TSource, TResult> previousParameters)
        {
            QueryParameters<TSource, TResult> result
               = ExpressionParser.ParseWhere<TSource, TResult, TWhere>(expression, previousParameters);
            return result;
            // TODO: add assertions to method ExpressionParserTest.ParseWhere(Expression`1<Func`2<!!2,Boolean>>, QueryParameters`2<!!0,!!1>)
        }

        [PexMethod]
        internal string ReversePropertyName(string propertyName)
        {
            string result = ExpressionParser.ReversePropertyName(propertyName);
            return result;
            // TODO: add assertions to method ExpressionParserTest.ReversePropertyName(String)
        }
    }
}

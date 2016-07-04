using System.Linq.Expressions;
// <copyright file="ExpressionFinderTest.cs">Copyright ©  2011</copyright>

using System;
using LinqToWiki.Expressions;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToWiki.Expressions.Tests
{
    [TestClass]
    [PexClass(typeof(ExpressionFinder))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedException(typeof(InvalidOperationException))]
    public partial class ExpressionFinderTest
    {

        [PexGenericArguments(typeof(Expression))]
        [PexMethod]
        public T Single<T>(Expression expression, Func<T, bool> condition)
            where T : Expression
        {
            T result = ExpressionFinder.Single<T>(expression, condition);
            return result;
            // TODO: add assertions to method ExpressionFinderTest.Single(Expression, Func`2<!!0,Boolean>)
        }
    }
}

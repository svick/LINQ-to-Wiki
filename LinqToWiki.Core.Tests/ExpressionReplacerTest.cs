using System.Linq.Expressions;
// <copyright file="ExpressionReplacerTest.cs">Copyright ©  2011</copyright>

using System;
using LinqToWiki.Expressions;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToWiki.Expressions.Tests
{
    [TestClass]
    [PexClass(typeof(ExpressionReplacer))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class ExpressionReplacerTest
    {

        [PexMethod]
        public Expression Replace(
            Expression expression,
            Expression toReplace,
            Expression replaceWith
        )
        {
            Expression result = ExpressionReplacer.Replace(expression, toReplace, replaceWith);
            return result;
            // TODO: add assertions to method ExpressionReplacerTest.Replace(Expression, Expression, Expression)
        }
    }
}

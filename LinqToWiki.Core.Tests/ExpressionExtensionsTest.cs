using System.Linq.Expressions;
// <copyright file="ExpressionExtensionsTest.cs">Copyright ©  2011</copyright>

using System;
using LinqToWiki.Expressions;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToWiki.Expressions.Tests
{
    [TestClass]
    [PexClass(typeof(ExpressionExtensions))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class ExpressionExtensionsTest
    {

        [PexMethod(MaxRunsWithoutNewTests = 200)]
        public BinaryExpression Switch(BinaryExpression expression)
        {
            BinaryExpression result = ExpressionExtensions.Switch(expression);
            return result;
            // TODO: add assertions to method ExpressionExtensionsTest.Switch(BinaryExpression)
        }
    }
}

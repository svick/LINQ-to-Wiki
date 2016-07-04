using System.Linq.Expressions;
// <copyright file="EnumFixerTest.cs">Copyright ©  2011</copyright>

using System;
using LinqToWiki.Expressions;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToWiki.Expressions.Tests
{
    [TestClass]
    [PexClass(typeof(EnumFixer))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class EnumFixerTest
    {

        [PexMethod(MaxRunsWithoutNewTests = 200)]
        internal Expression Fix(Expression expression)
        {
            Expression result = EnumFixer.Fix(expression);
            return result;
            // TODO: add assertions to method EnumFixerTest.Fix(Expression)
        }
    }
}

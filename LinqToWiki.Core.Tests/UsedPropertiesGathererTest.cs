using System.Collections.Generic;
using System.Linq.Expressions;
// <copyright file="UsedPropertiesGathererTest.cs">Copyright ©  2011</copyright>

using System;
using LinqToWiki.Expressions;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToWiki.Expressions.Tests
{
    [TestClass]
    [PexClass(typeof(UsedPropertiesGatherer))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class UsedPropertiesGathererTest
    {

        [PexMethod]
        internal void Gather(
            [PexAssumeUnderTest]UsedPropertiesGatherer target,
            Expression haystack,
            Expression needle,
            out bool usedDirectly, out IEnumerable<string> usedProperties
        )
        {
            target.Gather(haystack, needle);
            usedDirectly = target.UsedDirectly;
            usedProperties = target.UsedProperties;
        }

        [PexMethod]
        internal IEnumerable<string> UsedPropertiesGet([PexAssumeUnderTest]UsedPropertiesGatherer target)
        {
            IEnumerable<string> result = target.UsedProperties;
            return result;
            // TODO: add assertions to method UsedPropertiesGathererTest.UsedPropertiesGet(UsedPropertiesGatherer)
        }

        [PexMethod]
        internal Expression Visit([PexAssumeUnderTest]UsedPropertiesGatherer target, Expression node)
        {
            Expression result = target.Visit(node);
            return result;
            // TODO: add assertions to method UsedPropertiesGathererTest.Visit(UsedPropertiesGatherer, Expression)
        }
    }
}

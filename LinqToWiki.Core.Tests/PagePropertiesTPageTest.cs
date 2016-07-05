using System.Collections.Generic;
// <copyright file="PagePropertiesTPageTest.cs">Copyright ©  2011</copyright>

using System;
using LinqToWiki.Internals;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToWiki.Internals.Tests
{
    [TestClass]
    [PexClass(typeof(PageProperties<>))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class PagePropertiesTPageTest
    {

        [PexGenericArguments(typeof(int))]
        [PexMethod]
        internal Dictionary<string, QueryTypeProperties> PropertiesGet<TPage>()
        {
            Dictionary<string, QueryTypeProperties> result = PageProperties<TPage>.Properties;
            return result;
            // TODO: add assertions to method PagePropertiesTPageTest.PropertiesGet()
        }
    }
}

using System.Collections.Generic;
using System.Xml.Linq;
// <copyright file="PageDataTest.cs">Copyright ©  2011</copyright>

using System;
using LinqToWiki.Internals;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToWiki.Internals.Tests
{
    [TestClass]
    [PexClass(typeof(PageData))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class PageDataTest
    {

        [PexMethod]
        internal PageData Constructor(
            WikiInfo wiki,
            XElement element,
            Dictionary<string, QueryTypeProperties> pageProperties,
            PagingManager pagingManager
        )
        {
            PageData target = new PageData(wiki, element, pageProperties, pagingManager);
            return target;
            // TODO: add assertions to method PageDataTest.Constructor(WikiInfo, XElement, Dictionary`2<String,QueryTypeProperties>, PagingManager)
        }
    }
}

using LinqToWiki.Download;
using System.Collections.Generic;
using LinqToWiki.Parameters;
// <copyright file="PagingManagerTest.cs">Copyright ©  2011</copyright>

using System;
using LinqToWiki.Internals;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToWiki.Internals.Tests
{
    [TestClass]
    [PexClass(typeof(PagingManager))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class PagingManagerTest
    {

        [PexMethod]
        internal PagingManager Constructor(
            WikiInfo wiki,
            string generator,
            IEnumerable<PropQueryParameters> propQueryParametersCollection,
            IEnumerable<HttpQueryParameterBase> currentParameters,
            Dictionary<string, QueryTypeProperties> pageProperties,
            HttpQueryParameter primaryQueryContinue,
            Dictionary<string, HttpQueryParameter> secondaryQueryContinues
        )
        {
            PagingManager target = new PagingManager(wiki, generator, propQueryParametersCollection,
                                                     currentParameters, pageProperties, primaryQueryContinue, secondaryQueryContinues);
            return target;
            // TODO: add assertions to method PagingManagerTest.Constructor(WikiInfo, String, IEnumerable`1<PropQueryParameters>, IEnumerable`1<HttpQueryParameterBase>, Dictionary`2<String,QueryTypeProperties>, HttpQueryParameter, Dictionary`2<String,HttpQueryParameter>)
        }
    }
}

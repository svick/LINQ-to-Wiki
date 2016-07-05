using System.Collections.Generic;
using LinqToWiki.Parameters;
using LinqToWiki.Download;
// <copyright file="QueryPageProcessorTest.cs">Copyright ©  2011</copyright>

using System;
using LinqToWiki.Internals;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToWiki.Internals.Tests
{
    [TestClass]
    [PexClass(typeof(QueryPageProcessor))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class QueryPageProcessorTest
    {

        [PexMethod]
        [PexAllowedException(typeof(NullReferenceException))]
        [PexAllowedException(typeof(ArgumentNullException))]
        internal HttpQueryParameterBase[] ProcessParameters(
            IEnumerable<PropQueryParameters> propQueryParametersCollection,
            IEnumerable<HttpQueryParameterBase> currentParameters,
            Dictionary<string, QueryTypeProperties> pageProperties,
            bool withInfo,
            IEnumerable<string> includedProperties
        )
        {
            HttpQueryParameterBase[] result
                = QueryPageProcessor.ProcessParameters(propQueryParametersCollection, currentParameters,
                    pageProperties, withInfo, includedProperties);
            return result;
            // TODO: add assertions to method QueryPageProcessorTest.ProcessParameters(IEnumerable`1<PropQueryParameters>, IEnumerable`1<HttpQueryParameterBase>, Dictionary`2<String,QueryTypeProperties>, Boolean, IEnumerable`1<String>)
        }

        [PexMethod]
        public QueryPageProcessor Constructor(WikiInfo wiki)
        {
            QueryPageProcessor target = new QueryPageProcessor(wiki);
            return target;
            // TODO: add assertions to method QueryPageProcessorTest.Constructor(WikiInfo)
        }

        [PexGenericArguments(typeof(int))]
        [PexMethod]
        internal IEnumerable<TResult> ExecuteList<TResult>(
            [PexAssumeUnderTest]QueryPageProcessor target,
            PageQueryParameters parameters,
            Func<PageData, TResult> selector,
            Dictionary<string, QueryTypeProperties> pageProperties
        )
        {
            IEnumerable<TResult> result = target.ExecuteList<TResult>(parameters, selector, pageProperties);
            return result;
            // TODO: add assertions to method QueryPageProcessorTest.ExecuteList(QueryPageProcessor, PageQueryParameters, Func`2<PageData,!!0>, Dictionary`2<String,QueryTypeProperties>)
        }
    }
}

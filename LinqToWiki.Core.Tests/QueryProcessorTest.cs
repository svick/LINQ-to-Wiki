using LinqToWiki.Parameters;
using System.Collections.Generic;
using LinqToWiki.Download;
using System.Xml.Linq;
// <copyright file="QueryProcessorTest.cs">Copyright ©  2011</copyright>

using System;
using LinqToWiki.Internals;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToWiki.Internals.Tests
{
    [TestClass]
    [PexClass(typeof(QueryProcessor))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class QueryProcessorTest
    {

        [PexMethod]
        public XElement Download(
            WikiInfo wiki,
            IEnumerable<HttpQueryParameterBase> processedParameters,
            HttpQueryParameter queryContinue
        )
        {
            XElement result = QueryProcessor.Download(wiki, processedParameters, queryContinue);
            return result;
            // TODO: add assertions to method QueryProcessorTest.Download(WikiInfo, IEnumerable`1<HttpQueryParameterBase>, HttpQueryParameter)
        }

        [PexMethod]
        public XElement Download(
            WikiInfo wiki,
            IEnumerable<HttpQueryParameterBase> processedParameters,
            IEnumerable<HttpQueryParameter> queryContinues
        )
        {
            XElement result = QueryProcessor.Download(wiki, processedParameters, queryContinues);
            return result;
            // TODO: add assertions to method QueryProcessorTest.Download(WikiInfo, IEnumerable`1<HttpQueryParameterBase>, IEnumerable`1<HttpQueryParameter>)
        }

        [PexMethod]
        [PexAllowedException(typeof(NullReferenceException))]
        [PexAllowedException(typeof(InvalidOperationException))]
        public IEnumerable<HttpQueryParameterBase> ProcessParameters(
            QueryTypeProperties queryTypeProperties,
            QueryParameters parameters,
            bool list,
            bool generator,
            int limit
        )
        {
            IEnumerable<HttpQueryParameterBase> result
               = QueryProcessor.ProcessParameters(queryTypeProperties, parameters, list, generator, limit);
            return result;
            // TODO: add assertions to method QueryProcessorTest.ProcessParameters(QueryTypeProperties, QueryParameters, Boolean, Boolean, Int32)
        }

        [PexMethod]
        public HttpQueryParameter GetQueryContinue(XElement downloaded, string moduleName)
        {
            HttpQueryParameter result = QueryProcessor.GetQueryContinue(downloaded, moduleName);
            return result;
            // TODO: add assertions to method QueryProcessorTest.GetQueryContinue(XElement, String)
        }

        [PexMethod]
        public Dictionary<string, HttpQueryParameter> GetQueryContinues(XElement downloaded)
        {
            Dictionary<string, HttpQueryParameter> result = QueryProcessor.GetQueryContinues(downloaded);
            return result;
            // TODO: add assertions to method QueryProcessorTest.GetQueryContinues(XElement)
        }
    }
}

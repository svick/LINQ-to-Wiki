using LinqToWiki.Parameters;
using System.Collections.Generic;
// <copyright file="QueryProcessorTTest.cs">Copyright ©  2011</copyright>

using System;
using LinqToWiki.Internals;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToWiki.Internals.Tests
{
    [TestClass]
    [PexClass(typeof(QueryProcessor<>))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class QueryProcessorTTest
    {

        [PexGenericArguments(typeof(int))]
        [PexMethod]
        public QueryProcessor<T> Constructor<T>(WikiInfo wiki, QueryTypeProperties<T> queryTypeProperties)
        {
            QueryProcessor<T> target = new QueryProcessor<T>(wiki, queryTypeProperties);
            return target;
            // TODO: add assertions to method QueryProcessorTTest.Constructor(WikiInfo, QueryTypeProperties`1<!!0>)
        }

        [PexGenericArguments(typeof(int), typeof(int))]
        [PexMethod]
        public IEnumerable<TResult> ExecuteList<T, TResult>([PexAssumeUnderTest]QueryProcessor<T> target, QueryParameters<T, TResult> parameters)
        {
            IEnumerable<TResult> result = target.ExecuteList<TResult>(parameters);
            return result;
            // TODO: add assertions to method QueryProcessorTTest.ExecuteList(QueryProcessor`1<!!0>, QueryParameters`2<!!0,!!1>)
        }
    }
}

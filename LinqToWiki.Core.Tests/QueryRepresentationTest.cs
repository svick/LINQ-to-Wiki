using System.Reflection;
using LinqToWiki;
// <copyright file="QueryRepresentationTest.cs">Copyright ©  2011</copyright>

using System;
using LinqToWiki.Internals;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToWiki.Internals.Tests
{
    [TestClass]
    [PexClass(typeof(QueryRepresentation))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class QueryRepresentationTest
    {

        [PexMethod]
        public string ToQueryString(Namespace ns)
        {
            string result = QueryRepresentation.ToQueryString(ns);
            return result;
            // TODO: add assertions to method QueryRepresentationTest.ToQueryString(Namespace)
        }

        [PexMethod]
        public string ToQueryString(string s)
        {
            string result = QueryRepresentation.ToQueryString(s);
            return result;
            // TODO: add assertions to method QueryRepresentationTest.ToQueryString(String)
        }

        [PexMethod]
        public string ToQueryString(DateTime dateTime)
        {
            string result = QueryRepresentation.ToQueryString(dateTime);
            return result;
            // TODO: add assertions to method QueryRepresentationTest.ToQueryString(DateTime)
        }

        [PexMethod]
        public string ToQueryString(bool b)
        {
            string result = QueryRepresentation.ToQueryString(b);
            return result;
            // TODO: add assertions to method QueryRepresentationTest.ToQueryString(Boolean)
        }

        [PexMethod]
        public string ToQueryString(int i)
        {
            string result = QueryRepresentation.ToQueryString(i);
            return result;
            // TODO: add assertions to method QueryRepresentationTest.ToQueryString(Int32)
        }

        [PexMethod]
        public string ToQueryString(long l)
        {
            string result = QueryRepresentation.ToQueryString(l);
            return result;
            // TODO: add assertions to method QueryRepresentationTest.ToQueryString(Int64)
        }

        [PexMethod]
        public string ToQueryString(StringValue stringValue)
        {
            string result = QueryRepresentation.ToQueryString(stringValue);
            return result;
            // TODO: add assertions to method QueryRepresentationTest.ToQueryString(StringValue)
        }

        [PexMethod]
        [PexMethodUnderTest("ToQueryString(Object)")]
        internal string ToQueryString(object obj)
        {
            object[] args = new object[1];
            args[0] = obj;
            Type[] parameterTypes = new Type[1];
            parameterTypes[0] = typeof(object);
            string result0 = ((MethodBase)(typeof(QueryRepresentation).GetMethod("ToQueryString",
                                                                                 BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic, (Binder)null,
                                                                                 CallingConventions.Standard, parameterTypes, (ParameterModifier[])null)))
                                 .Invoke((object)null, args) as string;
            string result = result0;
            return result;
            // TODO: add assertions to method QueryRepresentationTest.ToQueryString(Object)
        }
    }
}

using System.Collections.Generic;
using LinqToWiki;
// <copyright file="NamespaceInfoTest.cs">Copyright ©  2011</copyright>

using System;
using LinqToWiki.Internals;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToWiki.Internals.Tests
{
    [TestClass]
    [PexClass(typeof(NamespaceInfo))]
    [PexAllowedException(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedException(typeof(NullReferenceException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class NamespaceInfoTest
    {

        [PexMethod]
        internal NamespaceInfo Constructor(IEnumerable<Namespace> namespaces)
        {
            NamespaceInfo target = new NamespaceInfo(namespaces);
            return target;
            // TODO: add assertions to method NamespaceInfoTest.Constructor(IEnumerable`1<Namespace>)
        }

        [PexMethod]
        public IEnumerator<Namespace> GetEnumerator([PexAssumeUnderTest]NamespaceInfo target)
        {
            IEnumerator<Namespace> result = target.GetEnumerator();
            return result;
            // TODO: add assertions to method NamespaceInfoTest.GetEnumerator(NamespaceInfo)
        }

        [PexMethod]
        [PexAllowedException(typeof(KeyNotFoundException))]
        public Namespace ItemGet([PexAssumeUnderTest]NamespaceInfo target, int id)
        {
            Namespace result = target[id];
            return result;
            // TODO: add assertions to method NamespaceInfoTest.ItemGet(NamespaceInfo, Int32)
        }
    }
}

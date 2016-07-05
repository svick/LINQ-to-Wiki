using System.Xml.Linq;
using System;
using Microsoft.Pex.Framework;

namespace System.Xml.Linq
{
    /// <summary>A factory for System.Xml.Linq.XName instances</summary>
    public static partial class XNameFactory
    {
        /// <summary>A factory for System.Xml.Linq.XName instances</summary>
        [PexFactoryMethod(typeof(XName))]
        public static object Create(string expandedName)
        {
            return XName.Get(expandedName);
        }

        /// <summary>A factory for System.Xml.Linq.XName instances</summary>
        [PexFactoryMethod(typeof(XName))]
        public static object Create(string localName, string namespaceName)
        {
            return XName.Get(localName, namespaceName);
        }
    }

    public partial class FactoryTest
    {
        [PexMethod]
        [PexAssertReachEventually(StopWhenAllReached = true)]
        public void XNameCreateTest(string name)
        {
            PexAssume.IsNotNullOrEmpty(name);
            var res = XNameFactory.Create(name);
            PexAssume.IsNotNull(res);
            PexAssert.ReachEventually();
        }
    }
}

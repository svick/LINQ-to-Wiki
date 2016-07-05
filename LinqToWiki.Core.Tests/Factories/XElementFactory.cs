using System.Xml.Linq;
// <copyright file="XElementFactory.cs">Copyright ©  2011</copyright>

using System;
using System.Collections.Generic;
using Microsoft.Pex.Framework;

namespace System.Xml.Linq
{
    /// <summary>A factory for System.Xml.Linq.XElement instances</summary>
    public static partial class XElementFactory
    {
        /// <summary>A factory for System.Xml.Linq.XElement instances</summary>
        [PexFactoryMethod(typeof(XElement))]
        public static XElement Create(XName name)
        {
            return new XElement(name);
        }

        /// <summary>A factory for System.Xml.Linq.XElement instances</summary>
        [PexFactoryMethod(typeof(XElement))]
        public static XElement Create(XName name, XObject content)
        {
            return new XElement(name, content);
        }

        /// <summary>A factory for System.Xml.Linq.XElement instances</summary>
        [PexFactoryMethod(typeof(XElement))]
        public static XElement Create(XName name, IEnumerable<XObject> content)
        {
            return new XElement(name, content);
        }
    }

    [PexClass]
    public partial class FactoryTest
    {
        [PexMethod]
        [PexAssertReachEventually(StopWhenAllReached = true)]
        public void XElementCreateTest(XName name)
        {
            PexAssume.IsNotNull(name);
            var res = XElementFactory.Create(name);
            PexAssume.IsNotNull(res);
            PexAssert.ReachEventually();
        }
    }
}

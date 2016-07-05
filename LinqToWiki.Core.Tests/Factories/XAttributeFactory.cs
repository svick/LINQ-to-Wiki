using System.Xml.Linq;
// <copyright file="XAttributeFactory.cs">Copyright ©  2011</copyright>

using System;
using Microsoft.Pex.Framework;

namespace System.Xml.Linq
{
    /// <summary>A factory for System.Xml.Linq.XAttribute instances</summary>
    public static partial class XAttributeFactory
    {
        /// <summary>A factory for System.Xml.Linq.XAttribute instances</summary>
        [PexFactoryMethod(typeof(XAttribute))]
        public static XAttribute Create(XName name, string value)
        {
            XAttribute xAttribute = new XAttribute(name, value);
            return xAttribute;
        }
    }
}

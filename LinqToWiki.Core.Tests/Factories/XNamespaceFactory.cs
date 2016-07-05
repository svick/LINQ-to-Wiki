using System.Xml.Linq;
// <copyright file="XNamespaceFactory.cs">Copyright ©  2011</copyright>

using System;
using Microsoft.Pex.Framework;

namespace System.Xml.Linq
{
    /// <summary>A factory for System.Xml.Linq.XNamespace instances</summary>
    public static partial class XNamespaceFactory
    {
        /// <summary>A factory for System.Xml.Linq.XNamespace instances</summary>
        [PexFactoryMethod(typeof(XNamespace))]
        public static XNamespace Create(string namespaceName)
        {
            return XNamespace.Get(namespaceName);
        }
    }
}

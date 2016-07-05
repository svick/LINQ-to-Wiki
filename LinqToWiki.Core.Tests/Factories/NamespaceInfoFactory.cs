using System.Collections.Generic;
using LinqToWiki;
using LinqToWiki.Internals;
// <copyright file="NamespaceInfoFactory.cs">Copyright ©  2011</copyright>

using System;
using Microsoft.Pex.Framework;

namespace LinqToWiki.Internals
{
    /// <summary>A factory for LinqToWiki.Internals.NamespaceInfo instances</summary>
    public static partial class NamespaceInfoFactory
    {
        /// <summary>A factory for LinqToWiki.Internals.NamespaceInfo instances</summary>
        [PexFactoryMethod(typeof(NamespaceInfo))]
        public static NamespaceInfo Create(IEnumerable<Namespace> namespaces_iEnumerable)
        {
            NamespaceInfo namespaceInfo = new NamespaceInfo(namespaces_iEnumerable);
            return namespaceInfo;
        }
    }
}

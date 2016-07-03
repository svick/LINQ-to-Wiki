using LinqToWiki;
// <copyright file="NamespaceFactory.cs">Copyright ©  2011</copyright>

using System;
using Microsoft.Pex.Framework;

namespace LinqToWiki
{
    /// <summary>A factory for LinqToWiki.Namespace instances</summary>
    public static partial class NamespaceFactory
    {
        /// <summary>A factory for LinqToWiki.Namespace instances</summary>
        [PexFactoryMethod(typeof(Namespace))]
        public static object Create(int id)
        {
            return Namespace.Get(id);
        }
    }
}

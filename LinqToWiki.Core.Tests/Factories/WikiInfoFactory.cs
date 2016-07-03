using System.Collections.Generic;
using LinqToWiki;
using LinqToWiki.Internals;
// <copyright file="WikiInfoFactory.cs">Copyright ©  2011</copyright>

using System;
using Microsoft.Pex.Framework;

namespace LinqToWiki.Internals
{
    /// <summary>A factory for LinqToWiki.Internals.WikiInfo instances</summary>
    public static partial class WikiInfoFactory
    {
        /// <summary>A factory for LinqToWiki.Internals.WikiInfo instances</summary>
        [PexFactoryMethod(typeof(WikiInfo))]
        public static WikiInfo Create(
            string userAgent_s,
            string baseUrl_s1,
            string apiPath_s2,
            IEnumerable<Namespace> namespaces_iEnumerable
        )
        {
            WikiInfo wikiInfo
               = new WikiInfo(userAgent_s, baseUrl_s1, apiPath_s2, namespaces_iEnumerable);
            return wikiInfo;
        }
    }
}

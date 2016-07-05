using System.Collections.Generic;
using System.Xml.Linq;
using LinqToWiki;
using LinqToWiki.Internals;
// <copyright file="PageDataFactory.cs">Copyright ©  2011</copyright>

using System;
using Microsoft.Pex.Framework;

namespace LinqToWiki.Internals
{
    /// <summary>A factory for LinqToWiki.Internals.PageData instances</summary>
    public static partial class PageDataFactory
    {
        /// <summary>A factory for LinqToWiki.Internals.PageData instances</summary>
        [PexFactoryMethod(typeof(IFirst), "LinqToWiki.Internals.PageData")]
        internal static PageData Create(
            WikiInfo wiki_wikiInfo,
            XElement element_xElement,
            Dictionary<string, QueryTypeProperties> pageProperties_dictionary,
            PagingManager pagingManager_pagingManager
        )
        {
            PageData pageData = new PageData(wiki_wikiInfo, element_xElement,
                                             pageProperties_dictionary, pagingManager_pagingManager);
            return pageData;
        }
    }
}

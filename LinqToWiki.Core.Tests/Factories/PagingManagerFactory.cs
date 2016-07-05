using LinqToWiki.Download;
using System.Collections.Generic;
using LinqToWiki.Parameters;
using LinqToWiki;
using LinqToWiki.Internals;
// <copyright file="PagingManagerFactory.cs">Copyright ©  2011</copyright>

using System;
using Microsoft.Pex.Framework;

namespace LinqToWiki.Internals
{
    /// <summary>A factory for LinqToWiki.Internals.PagingManager instances</summary>
    public static partial class PagingManagerFactory
    {
        /// <summary>A factory for LinqToWiki.Internals.PagingManager instances</summary>
        [PexFactoryMethod(typeof(IFirst), "LinqToWiki.Internals.PagingManager")]
        internal static PagingManager Create(
            WikiInfo wiki_wikiInfo,
            string generator_s,
            IEnumerable<PropQueryParameters> propQueryParametersCollection_iEnumerable,
            IEnumerable<HttpQueryParameterBase> currentParameters_iEnumerable1_,
            Dictionary<string, QueryTypeProperties> pageProperties_dictionary,
            HttpQueryParameter primaryQueryContinue_httpQueryParameter,
            Dictionary<string, HttpQueryParameter> secondaryQueryContinues_dictionary1_,
            IEnumerable<PageData> pages_iEnumerable2_
        )
        {
            PexAssume.IsNotNull(wiki_wikiInfo);
            PexAssume.IsNotNull(generator_s);
            PexAssume.IsNotNull(propQueryParametersCollection_iEnumerable);
            PexAssume.IsNotNull(currentParameters_iEnumerable1_);
            PexAssume.IsNotNull(pageProperties_dictionary);
            PexAssume.IsNotNull(primaryQueryContinue_httpQueryParameter);
            PexAssume.IsNotNull(secondaryQueryContinues_dictionary1_);
            PexAssume.IsNotNull(pages_iEnumerable2_);

            PagingManager pagingManager = new PagingManager
                                              (wiki_wikiInfo, generator_s, propQueryParametersCollection_iEnumerable,
                                               currentParameters_iEnumerable1_, pageProperties_dictionary,
                                               primaryQueryContinue_httpQueryParameter,
                                               secondaryQueryContinues_dictionary1_);
            pagingManager.SetPages(pages_iEnumerable2_);
            return pagingManager;
        }
    }
}

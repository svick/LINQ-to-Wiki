using System.Xml.Linq;
using System.Collections.Generic;
using LinqToWiki.Internals;
// <copyright file="QueryTypePropertiesFactory.cs">Copyright ©  2011</copyright>

using System;
using Microsoft.Pex.Framework;

namespace LinqToWiki.Internals
{
    /// <summary>A factory for LinqToWiki.Internals.QueryTypeProperties`1[System.Int32] instances</summary>
    public static partial class QueryTypePropertiesFactory
    {
        /// <summary>A factory for LinqToWiki.Internals.QueryTypeProperties`1[System.Int32] instances</summary>
        [PexFactoryMethod(typeof(QueryTypeProperties<int>))]
        public static QueryTypeProperties<int> Create(
            string moduleName_s,
            string prefix_s1,
            QueryType? queryType_nulli,
            SortType? sortType_nulli1_,
            IEnumerable<Tuple<string, string>> baseParameters_iEnumerable,
            IDictionary<string, string[]> props_iDictionary,
            Func<XElement, WikiInfo, int> parser_func
        )
        {
            PexAssume.IsNotNull(baseParameters_iEnumerable);
            QueryTypeProperties<int> queryTypeProperties
               = new QueryTypeProperties<int>(moduleName_s, prefix_s1, queryType_nulli,
                                              sortType_nulli1_, baseParameters_iEnumerable,
                                              props_iDictionary, parser_func);
            return queryTypeProperties;
        }
    }
}

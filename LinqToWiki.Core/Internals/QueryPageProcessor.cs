using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LinqToWiki.Collections;
using LinqToWiki.Download;
using LinqToWiki.Parameters;

namespace LinqToWiki.Internals
{
    /// <summary>
    /// Handles executing of page source queries.
    /// </summary>
    public class QueryPageProcessor
    {
        private readonly WikiInfo m_wiki;

        public QueryPageProcessor(WikiInfo wiki)
        {
            m_wiki = wiki;
        }

        /// <summary>
        /// Returns the results of the query as a lazy collection.
        /// </summary>
        internal IEnumerable<TResult> ExecuteList<TResult>(
            PageQueryParameters parameters, Func<PageData, TResult> selector,
            Dictionary<string, QueryTypeProperties> pageProperties)
        {
            HttpQueryParameter primaryQueryContinue = null;

            var revisions = parameters.PropQueryParametersCollection.SingleOrDefault(p => p.PropName == "revisions");

            int limit = (revisions == null || revisions.OnlyFirst) ? -1 : 1;
            var pagesCollection = parameters.PagesCollection;

            do
            {
                var currentParameters = pagesCollection.GetNextPage(limit).ToArray();
                var processedParameters = ProcessParameters(
                    parameters.PropQueryParametersCollection, currentParameters, pageProperties);
                var generatorParameter = (HttpQueryParameter)currentParameters.SingleOrDefault(p => p.Name == "generator");
                var generator = generatorParameter == null ? null : generatorParameter.Value;

                var downloaded = QueryProcessor.Download(m_wiki, processedParameters, primaryQueryContinue);

                var queryContinues = QueryProcessor.GetQueryContinues(downloaded);

                HttpQueryParameter newPrimaryQueryContinue = null;

                if (generator != null)
                {
                    queryContinues.TryGetValue(generator, out newPrimaryQueryContinue);

                    queryContinues.Remove(generator);
                }

                var pagingManager = new PagingManager(
                    m_wiki, generator, parameters.PropQueryParametersCollection, currentParameters, pageProperties,
                    primaryQueryContinue, queryContinues);

                var queryElement = downloaded.Element("query");

                if (queryElement != null)
                {
                    var partPageData = queryElement.Element("pages").Elements("page")
                        .Select(e => new PageData(m_wiki, e, pageProperties, pagingManager)).ToArray();

                    pagingManager.SetPages(partPageData);

                    var part = partPageData.Select(selector);

                    foreach (var item in part)
                        yield return item;
                }

                primaryQueryContinue = newPrimaryQueryContinue;
            } while (pagesCollection.HasMorePages(primaryQueryContinue));
        }

        /// <summary>
        /// Processes the data about the query and returns a collection of query parameters.
        /// </summary>
        internal static HttpQueryParameterBase[] ProcessParameters(
            IEnumerable<PropQueryParameters> propQueryParametersCollection,
            IEnumerable<HttpQueryParameterBase> currentParameters,
            Dictionary<string, QueryTypeProperties> pageProperties,
            bool withInfo = true, IEnumerable<string> includedProperties = null)
        {
            Contract.Requires(currentParameters != null);
            Contract.Requires(pageProperties != null);
            Contract.Ensures(Contract.Result<object>() != null);

            var propParameters = new List<HttpQueryParameterBase>();

            var propNames = new List<string>();

            if (!withInfo)
                propQueryParametersCollection = propQueryParametersCollection.Where(x => x.PropName != "info");

            if (includedProperties != null)
                propQueryParametersCollection =
                    propQueryParametersCollection.Where(x => includedProperties.Contains(x.PropName));

            foreach (var propQueryParameters in propQueryParametersCollection)
            {
                Contract.Assume(propQueryParameters != null);

                propNames.Add(propQueryParameters.PropName);

                var queryTypeProperties = pageProperties[propQueryParameters.PropName];

                Contract.Assume(queryTypeProperties != null);

                propParameters.AddRange(
                    QueryProcessor.ProcessParameters(
                        queryTypeProperties, propQueryParameters, true,
                        limit: propQueryParameters.OnlyFirst ? 0 : -1));
            }

            return new[] { new HttpQueryParameter("action", "query") }
                .Concat(currentParameters)
                .Concat(new[] { new HttpQueryParameter("prop", propNames.ToQueryString()) })
                .Concat(propParameters)
                .ToArray();
        }
    }
}
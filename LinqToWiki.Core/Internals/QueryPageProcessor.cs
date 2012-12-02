using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Collections;
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
            Tuple<string, string> primaryQueryContinue = null;

            var revisions = parameters.PropQueryParametersCollection.SingleOrDefault(p => p.PropName == "revisions");

            int limit = (revisions == null || revisions.OnlyFirst) ? -1 : 1;
            var pagesCollection = parameters.PagesCollection;

            do
            {
                var currentParameters = pagesCollection.GetNextPage(limit).ToArray();
                var processedParameters = ProcessParameters(
                    parameters.PropQueryParametersCollection, currentParameters, pageProperties);
                var generatorParameter = currentParameters.SingleOrDefault(p => p.Item1 == "generator");
                var generator = generatorParameter == null ? null : generatorParameter.Item2;

                var downloaded = QueryProcessor.Download(m_wiki, processedParameters, primaryQueryContinue);

                var queryContinues = QueryProcessor.GetQueryContinues(downloaded);

                Tuple<string, string> newPrimaryQueryContinue = null;

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
        internal static Tuple<string, string>[] ProcessParameters(
            IEnumerable<PropQueryParameters> propQueryParametersCollection,
            IEnumerable<Tuple<string, string>> currentParameters, Dictionary<string, QueryTypeProperties> pageProperties,
            bool withInfo = true)
        {
            var propParameters = new TupleList<string, string>();

            var propNames = new List<string>();

            if (!withInfo)
                propQueryParametersCollection = propQueryParametersCollection.Where(x => x.PropName != "info");

            foreach (var propQueryParameters in propQueryParametersCollection)
            {
                propNames.Add(propQueryParameters.PropName);

                propParameters.AddRange(
                    QueryProcessor.ProcessParameters(
                        pageProperties[propQueryParameters.PropName], propQueryParameters, true,
                        limit: propQueryParameters.OnlyFirst ? 0 : -1));
            }

            return new[] { Tuple.Create("action", "query") }
                .Concat(currentParameters.Select(x => Tuple.Create(x.Item1, x.Item2)))
                .Concat(new[] { Tuple.Create("prop", propNames.ToQueryString()) })
                .Concat(propParameters)
                .ToArray();
        }
    }
}
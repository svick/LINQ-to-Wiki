using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Collections;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    public class QueryPageProcessor
    {
        private readonly WikiInfo m_wiki;

        public QueryPageProcessor(WikiInfo wiki)
        {
            m_wiki = wiki;
        }

        internal IEnumerable<TResult> ExecuteList<TResult>(
            PageQueryParameters parameters, Func<PageData, TResult> selector,
            Dictionary<string, QueryTypeProperties> pageProperties)
        {
            var processedParameters = ProcessParameters(parameters, pageProperties);

            Tuple<string, string> primaryQueryContinue = null;
            var generatorParameter = parameters.Value.SingleOrDefault(p => p.Name == "generator");
            var generator = generatorParameter == null ? null : generatorParameter.Value;

            do
            {
                var downloaded = QueryProcessor.Download(m_wiki, processedParameters, primaryQueryContinue);

                var queryContinues = QueryProcessor.GetQueryContinues(downloaded);

                Tuple<string, string> newPrimaryQueryContinue = null;

                if (generator != null)
                {
                    queryContinues.TryGetValue(generator, out newPrimaryQueryContinue);

                    queryContinues.Remove(generator);
                }

                var pagingManager = new PagingManager(m_wiki, generator, parameters, pageProperties, primaryQueryContinue, queryContinues);

                var queryElement = downloaded.Element("query");

                if (queryElement == null)
                    yield break;

                var partPageData = queryElement.Element("pages").Elements("page")
                    .Select(e => new PageData(m_wiki, e, pageProperties, pagingManager)).ToArray();

                pagingManager.SetPages(partPageData);

                var part = partPageData.Select(selector);

                foreach (var item in part)
                    yield return item;

                primaryQueryContinue = newPrimaryQueryContinue;
            } while (primaryQueryContinue != null);
        }

        internal static Tuple<string, string>[] ProcessParameters(
            PageQueryParameters parameters, Dictionary<string, QueryTypeProperties> pageProperties, bool withInfo = true)
        {
            var propParameters = new TupleList<string, string>();

            var propNames = new List<string>();

            var propQueryParametersCollection = parameters.PropQueryParametersCollection;

            if (!withInfo)
                propQueryParametersCollection = propQueryParametersCollection.Where(x => x.PropName != "info");

            foreach (var propQueryParameters in propQueryParametersCollection)
            {
                propNames.Add(propQueryParameters.PropName);

                propParameters.AddRange(
                    QueryProcessor.ProcessParameters(pageProperties[propQueryParameters.PropName], propQueryParameters, true));
            }

            return new[] { Tuple.Create("action", "query") }
                .Concat(parameters.Value.Select(nvp => Tuple.Create(nvp.Name, nvp.Value)))
                .Concat(new[] { Tuple.Create("prop", NameValueParameter.JoinValues(propNames)) })
                .Concat(propParameters)
                .ToArray();
        }
    }
}
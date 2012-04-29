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
            var propParameters = new TupleList<string, string>();

            var propNames = new List<string>();

            foreach (var propQueryParameters in parameters.PropQueryParametersCollection)
            {
                propNames.Add(propQueryParameters.PropName);

                propParameters.AddRange(
                    QueryProcessor.ProcessParameters(pageProperties[propQueryParameters.PropName], propQueryParameters, true));
            }

            var processedParameters = new[] { Tuple.Create("action", "query") }
                .Concat(parameters.Value.Select(nvp => Tuple.Create(nvp.Name, nvp.Value)))
                .Concat(new[] { Tuple.Create("prop", NameValueParameter.JoinValues(propNames)) })
                .Concat(propParameters);

            Tuple<string, string> queryContinue = null;
            var generatorParameter = parameters.Value.SingleOrDefault(p => p.Name == "generator");
            var generator = generatorParameter == null ? null : generatorParameter.Value;

            do
            {
                var downloaded = QueryProcessor.Download(m_wiki, processedParameters, queryContinue);

                var part = downloaded.Element("query").Element("pages").Elements("page")
                    .Select(e => selector(new PageData(m_wiki, e, pageProperties)));

                foreach (var item in part)
                    yield return item;

                queryContinue = QueryProcessor.GetQueryContinue(downloaded, generator);
            } while (queryContinue != null);
        }
    }
}
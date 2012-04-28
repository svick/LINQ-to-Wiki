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

        public IEnumerable<TResult> ExecuteList<TResult>(
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

            var element = QueryProcessor.Download(m_wiki, processedParameters);

            return element.Element("query").Element("pages").Elements("page")
                .Select(e => selector(new PageData(m_wiki, e, pageProperties)));
        }
    }
}
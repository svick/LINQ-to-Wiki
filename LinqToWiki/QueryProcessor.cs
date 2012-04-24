using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LinqToWiki.Collections;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    /// <summary>
    /// Processes query parameters and parses the result of the query.
    /// </summary>
    public class QueryProcessor<T>
    {
        private readonly WikiInfo m_wiki;
        private readonly QueryTypeProperties<T> m_queryTypeProperties;

        public QueryProcessor(WikiInfo wiki, QueryTypeProperties<T> queryTypeProperties)
        {
            m_wiki = wiki;
            m_queryTypeProperties = queryTypeProperties;
        }

        /// <summary>
        /// Executes a query based on the <see cref="parameters"/> and returns a collection of results.
        /// </summary>
        public IEnumerable<TResult> ExecuteList<TResult>(QueryParameters<T, TResult> parameters)
        {
            var downloaded = Download(parameters, true);

            switch (m_queryTypeProperties.QueryType)
            {
            case QueryType.List:
            case QueryType.Meta:
                var moduleElement = downloaded.Element("query").Element(m_queryTypeProperties.ModuleName);
                var resultsElement = moduleElement.Element("results") ?? moduleElement;
                return resultsElement
                    .Elements()
                    .Select(x => parameters.Selector(m_queryTypeProperties.Parse(x, m_wiki)));
            case QueryType.Prop:
            case null:
                throw new NotImplementedException();
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Executes a query based on the <see cref="parameters"/> and returns a single result.
        /// </summary>
        public TResult ExecuteSingle<TResult>(QueryParameters<T, TResult> parameters)
        {
            var downloaded = Download(parameters, false);

            switch (m_queryTypeProperties.QueryType)
            {
            case QueryType.List:
                throw new InvalidOperationException();
            case QueryType.Prop:
                throw new NotImplementedException();
            case QueryType.Meta:
                return parameters.Selector(
                    m_queryTypeProperties.Parse(downloaded.Element("query"), m_wiki));
            case null:
                return
                    parameters.Selector(
                        m_queryTypeProperties.Parse(
                            downloaded.Element(m_queryTypeProperties.ModuleName),
                            m_wiki));
            }

            throw new NotSupportedException();
        }


        private XElement Download<TResult>(QueryParameters<T, TResult> parameters, bool list)
        {
            // TODO: too long, split

            var parsedParameters = new TupleList<string, string>(m_queryTypeProperties.BaseParameters);

            string prefix = m_queryTypeProperties.Prefix;

            if (parameters.Value != null)
                foreach (var value in parameters.Value)
                    parsedParameters.Add(prefix + value.Name, value.Value);

            if (parameters.Ascending != null)
            {
                if (parameters.Sort != null)
                    parsedParameters.Add(prefix + "sort", parameters.Sort);

                string dir;
                switch (m_queryTypeProperties.SortType)
                {
                case SortType.Ascending:
                    dir = parameters.Ascending.Value ? "ascending" : "descending";
                    break;
                case SortType.Newer:
                    dir = parameters.Ascending.Value ? "newer" : "older";
                    break;
                default:
                    throw new InvalidOperationException();
                }

                parsedParameters.Add(prefix + "dir", dir);
            }

            var selectedProps = new List<string>();

            if (parameters.Properties == null)
                selectedProps.AddRange(m_queryTypeProperties.GetAllProps().Except(new[] { "" }));
            else
            {
                var requiredPropsCollection =
                    parameters.Properties
                        .Select(m_queryTypeProperties.GetProps)
                        .Where(ps => !ps.SequenceEqual(new[] { "" }))
                        .OrderBy(ps => ps.Length);
                foreach (var props in requiredPropsCollection)
                {
                    if (!props.Intersect(selectedProps).Any())
                        selectedProps.Add(props.First());
                }
            }

            if (list)
            {
                parsedParameters.Add(prefix + "prop", NameValueParameter.JoinValues(selectedProps));

                parsedParameters.Add(prefix + "limit", "max");
            }

            // TODO: add paging, maxlag

            var downloaded = m_wiki.Downloader.Download(parsedParameters);

            var root = downloaded.Root;

            var error = root.Element("error");
            if (error != null)
                throw ParseError(error);

            return root;
        }

        private static Exception ParseError(XElement error)
        {
            return new ApiErrorException((string)error.Attribute("code"), (string)error.Attribute("info"));
        }
    }
}
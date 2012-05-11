using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LinqToWiki.Collections;
using LinqToWiki.Parameters;

namespace LinqToWiki.Internals
{
    /// <summary>
    /// Processes query parameters and parses the result of the query.
    /// </summary>
    public class QueryProcessor<T> : QueryProcessor
    {
        private readonly WikiInfo m_wiki;
        private readonly QueryTypeProperties<T> m_queryTypeProperties;

        public QueryProcessor(WikiInfo wiki, QueryTypeProperties<T> queryTypeProperties)
        {
            m_wiki = wiki;
            m_queryTypeProperties = queryTypeProperties;
        }

        /// <summary>
        /// Executes a query based on the <see cref="parameters"/> and returns a lazy collection of results.
        /// </summary>
        public IEnumerable<TResult> ExecuteList<TResult>(QueryParameters<T, TResult> parameters)
        {
            var processedParameters = ProcessParameters(parameters, true).ToArray();

            Tuple<string, string> queryContinue = null;

            do
            {
                var downloaded = Download(processedParameters, queryContinue);

                var part = GetListItems(parameters.Selector, downloaded);

                foreach (var item in part)
                    yield return item;

                queryContinue = GetQueryContinue(downloaded, m_queryTypeProperties.ModuleName);
            } while (queryContinue != null);
        }

        /// <summary>
        /// Retrieves items for a single primary page.
        /// </summary>
        private IEnumerable<TResult> GetListItems<TResult>(Func<T, TResult> selector, XElement downloaded)
        {
            XElement resultsElement;
            switch (m_queryTypeProperties.QueryType)
            {
            case QueryType.List:
            case QueryType.Meta:
                var moduleElement = downloaded.Element("query").Element(m_queryTypeProperties.ModuleName);
                resultsElement = moduleElement.Element("results") ?? moduleElement;
                break;
            case null:
                resultsElement = downloaded.Element(m_queryTypeProperties.ModuleName);
                break;
            default:
                throw new NotSupportedException();
            }

            return resultsElement
                .Elements()
                .Select(x => selector(m_queryTypeProperties.Parse(x, m_wiki)));
        }

        /// <summary>
        /// Executes a query based on the <see cref="parameters"/> and returns a single result.
        /// </summary>
        public TResult ExecuteSingle<TResult>(QueryParameters<T, TResult> parameters)
        {
            var downloaded = Download(ProcessParameters(parameters, false));

            switch (m_queryTypeProperties.QueryType)
            {
            case QueryType.List:
            case QueryType.Prop:
                throw new InvalidOperationException();
            case QueryType.Meta:
                return parameters.Selector(m_queryTypeProperties.Parse(downloaded.Element("query"), m_wiki));
            case null:
                var element = downloaded.Element(m_queryTypeProperties.ModuleName);
                var attribute = downloaded.Attribute(m_queryTypeProperties.ModuleName);
                if (element == null && attribute != null)
                    element = new XElement(attribute.Name, attribute.Value);
                return
                    parameters.Selector(m_queryTypeProperties.Parse(element, m_wiki));
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Executes query based on the given parameters and returns the results as an XML element.
        /// </summary>
        private XElement Download(
            IEnumerable<Tuple<string, string>> processedParameters, Tuple<string, string> queryContinue = null)
        {
            return Download(m_wiki, processedParameters, queryContinue);
        }

        /// <summary>
        /// Processes the data about the query and returns a collection of query parameters.
        /// </summary>
        private IEnumerable<Tuple<string, string>> ProcessParameters(QueryParameters parameters, bool list)
        {
            return ProcessParameters(m_queryTypeProperties, parameters, list);
        }

        /// <summary>
        /// Returns a <see cref="QueryPageProcessor"/> used in page source queries.
        /// </summary>
        public QueryPageProcessor GetPageProcessor()
        {
            return new QueryPageProcessor(m_wiki);
        }

        /// <summary>
        /// Returns a delegate that can be used to get generator parameters for a given limit.
        /// </summary>
        public Func<int, IEnumerable<Tuple<string, string>>> ProcessGeneratorParameters(QueryParameters parameters)
        {
            return limit => ProcessParameters(m_queryTypeProperties, parameters, true, true, limit);
        }
    }

    /// <summary>
    /// Processes query parameters and parses the result of the query.
    /// Non-generic, static part of <see cref="QueryProcessor{T}"/>.
    /// </summary>
    public abstract class QueryProcessor
    {
        /// <summary>
        /// Executes query based on the given parameters and returns the results as an XML element.
        /// </summary>
        public static XElement Download(
            WikiInfo wiki, IEnumerable<Tuple<string, string>> processedParameters,
            Tuple<string, string> queryContinue = null)
        {
            return Download(wiki, processedParameters, new[] { queryContinue });
        }

        /// <summary>
        /// Executes query based on the given parameters and returns the results as an XML element.
        /// Supports multiple query-contine parameters.
        /// </summary>
        public static XElement Download(
            WikiInfo wiki, IEnumerable<Tuple<string, string>> processedParameters,
            IEnumerable<Tuple<string, string>> queryContinues = null)
        {
            if (queryContinues != null)
                processedParameters = processedParameters.Concat(queryContinues.Where(x => x != null));

            int i = 1;

            while (true)
            {
                var downloaded = wiki.Downloader.Download(processedParameters);

                var root = downloaded.Root;

                var error = root.Element("error");
                if (error != null)
                {
                    var exception = ParseError(error);
                    if (exception.Code == "maxlag")
                    {
                        if (Downloader.LogDownloading)
                            Console.WriteLine(exception.Message);
                        var waitTime = TimeSpan.FromSeconds(Math.Pow(2, i) * 5);
                        System.Threading.Thread.Sleep(waitTime);
                        continue;
                    }

                    throw exception;
                }

                return root;
            }
        }

        /// <summary>
        /// Parses error element returned from the API into an <see cref="ApiErrorException"/>.
        /// </summary>
        private static ApiErrorException ParseError(XElement error)
        {
            return new ApiErrorException((string)error.Attribute("code"), (string)error.Attribute("info"));
        }

        /// <summary>
        /// Processes the data about the query and returns a collection of query parameters.
        /// </summary>
        public static IEnumerable<Tuple<string, string>> ProcessParameters(
            QueryTypeProperties queryTypeProperties, QueryParameters parameters,
            bool list, bool generator = false, int limit = -1)
        {
            var parsedParameters = new TupleList<string, string>();

            if (generator)
            {
                var generatorParameter = queryTypeProperties.BaseParameters.Single(p => p.Item1 != "action");
                parsedParameters.Add("generator", generatorParameter.Item2);
            }
            else
                parsedParameters.AddRange(queryTypeProperties.BaseParameters);

            string prefix = queryTypeProperties.Prefix;

            if (generator)
                prefix = 'g' + prefix;

            if (parameters.Value != null)
                foreach (var value in parameters.Value)
                    parsedParameters.Add(prefix + value.Name, value.Value);

            if (parameters.Ascending != null)
            {
                if (parameters.Sort != null)
                    parsedParameters.Add(prefix + "sort", parameters.Sort);

                string dir;
                switch (queryTypeProperties.SortType)
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
                selectedProps.AddRange(queryTypeProperties.GetAllProps().Except(new[] { "" }));
            else
            {
                var requiredPropsCollection =
                    parameters.Properties
                        .Select(queryTypeProperties.GetProps)
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
                if (!generator)
                    parsedParameters.Add(prefix + "prop", selectedProps.ToQueryString());

                if (limit != 0)
                    parsedParameters.Add(prefix + "limit", limit == -1 ? "max" : limit.ToQueryString());
            }

            return parsedParameters;
        }

        /// <summary>
        /// Gets query-continue values for a given module.
        /// </summary>
        public static Tuple<string, string> GetQueryContinue(XElement downloaded, string moduleName)
        {
            Tuple<string, string> result;
            GetQueryContinues(downloaded).TryGetValue(moduleName, out result);
            return result;
        }

        /// <summary>
        /// Parses query-continue values in a query result.
        /// </summary>
        public static Dictionary<string, Tuple<string, string>> GetQueryContinues(XElement downloaded)
        {
            var queryContinueElement = downloaded.Element("query-continue");

            if (queryContinueElement == null)
                return new Dictionary<string, Tuple<string, string>>();

            var listContinueElements = queryContinueElement.Elements();
            var listContinueAttributes = listContinueElements.Attributes();
            return listContinueAttributes.ToDictionary(
                a => a.Parent.Name.LocalName, a => Tuple.Create(a.Name.LocalName, a.Value));
        }
    }
}
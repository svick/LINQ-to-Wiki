using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using LinqToWiki.Internals;
using RestSharp;

namespace LinqToWiki.Download
{
    /// <summary>
    /// Downloads the results of a query from the wiki web site.
    /// </summary>
    public class Downloader
    {
        static Downloader()
        {
            ServicePointManager.Expect100Continue = false;
            UseMaxlag = true;
        }

        /// <summary>
        /// Whether to set the <c>maxlag</c> parameter to limit queries in times of high load.
        /// See http://www.mediawiki.org/wiki/Manual:Maxlag_parameter.
        /// </summary>
        public static bool UseMaxlag { get; set; }

        /// <summary>
        /// Whether each request should be logged to the console.
        /// </summary>
        public static bool LogDownloading { get; set; }

        /// <summary>
        /// The value of the <c>User-Agent</c> header of requests.
        /// </summary>
        public string UserAgent { get { return string.Format("{0} LinqToWiki", m_wiki.UserAgent); } }

        private readonly WikiInfo m_wiki;
        private readonly CookieContainer m_cookies = new CookieContainer();

        public Downloader(WikiInfo wiki)
        {
            if (wiki == null)
                throw new ArgumentNullException(nameof(wiki));

            m_wiki = wiki;
        }

        /// <summary>
        /// Downloads the results of query defined by <paramref name="parameters"/>.
        /// </summary>
        public XDocument Download(IEnumerable<HttpQueryParameterBase> parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            parameters = parameters.ToArray();

            if (LogDownloading)
                LogRequest(parameters);

            parameters = new[] { new HttpQueryParameter("format", "xml") }.Concat(parameters);

            if (UseMaxlag)
                parameters = parameters.Concat(new[] { new HttpQueryParameter("maxlag", "5") });

            var client = new RestClient
            {
                BaseUrl = new Uri(m_wiki.ApiUrl.AbsoluteUri + "?rawcontinue"),
                CookieContainer = m_cookies,
                UserAgent = UserAgent
            };
            var request = new RestRequest(Method.POST);

            WriteParameters(parameters, request);

            var response = client.Execute(request);

            return XDocument.Parse(response.Content);
        }

        /// <summary>
        /// Logs the request to the console.
        /// </summary>
        private void LogRequest(IEnumerable<HttpQueryParameterBase> parameters)
        {
            Console.WriteLine("{0}?{1}", m_wiki.ApiUrl, string.Join("&", parameters));
        }

        /// <summary>
        /// Writes parameters to a request.
        /// </summary>
        private static void WriteParameters(IEnumerable<HttpQueryParameterBase> parameters, RestRequest request)
        {
            foreach (var parameter in parameters)
            {
                var normalParameter = parameter as HttpQueryParameter;
                if (normalParameter != null)
                {
                    request.AddParameter(normalParameter.Name, normalParameter.Value);
                    continue;
                }
                var fileParameter = parameter as HttpQueryFileParameter;
                if (fileParameter != null)
                {
                    request.AddFile(fileParameter.Name, stream => fileParameter.File.CopyTo(stream), "noname");
                }
            }
        }
    }
}
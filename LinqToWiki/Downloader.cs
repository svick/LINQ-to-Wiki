using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Linq;
using System.Linq;
using LinqToWiki.Internals;

namespace LinqToWiki
{
    /// <summary>
    /// Downloads the results of a query from the wiki web site.
    /// </summary>
    public class Downloader
    {
        static Downloader()
        {
            ServicePointManager.Expect100Continue = false;
            UserAgent = "Linq to Wiki by [[w:en:User:Svick]]";
            UseMaxlag = true;
        }

        /// <summary>
        /// The value of the <c>User-Agent</c> header of requests.
        /// Should be set to “an informative User-Agent string with contact information”.
        /// See the <see cref="http://meta.wikimedia.org/wiki/User-Agent_policy">WikiMedia User-Agent policy</see>.
        /// </summary>
        public static string UserAgent { get; set; }

        /// <summary>
        /// Whether to set the <c>maxlag</c> parameter to limit queries in times of high load.
        /// See <see cref="http://www.mediawiki.org/wiki/Manual:Maxlag_parameter">Maxlag parameter in the MediaWiki manual</see>.
        /// </summary>
        public static bool UseMaxlag { get; set; }

        /// <summary>
        /// Whether each request should be logged to the console.
        /// </summary>
        public static bool LogDownloading { get; set; }

        private readonly WikiInfo m_wiki;
        private readonly CookieContainer m_cookies = new CookieContainer();

        public Downloader(WikiInfo wiki)
        {
            m_wiki = wiki;
        }

        /// <summary>
        /// Downloads the results of query defined by <see cref="parameters"/>.
        /// </summary>
        public XDocument Download(IEnumerable<Tuple<string, string>> parameters)
        {
            if (LogDownloading)
                LogRequest(parameters);

            parameters = new[] { Tuple.Create("format", "xml") }.Concat(parameters);

            if (UseMaxlag)
                parameters = parameters.Concat(new[] { Tuple.Create("maxlag", "5") });

            var request = CreateRequest();

            using (var requestStream = request.GetRequestStream())
            using (var requestWriter = new StreamWriter(requestStream))
            {
                WriteParameters(parameters, requestWriter);
            }

            var response = request.GetResponse();

            return XDocument.Load(response.GetResponseStream());
        }

        /// <summary>
        /// Logs the request to the console.
        /// </summary>
        private void LogRequest(IEnumerable<Tuple<string, string>> parameters)
        {
            Console.Write(m_wiki.ApiUrl);
            Console.Write(": ");
            Console.WriteLine(string.Join(" & ", parameters.Select(p => p.Item1 + '=' + p.Item2)));
        }

        /// <summary>
        /// Creates a POST request.
        /// </summary>
        private HttpWebRequest CreateRequest()
        {
            var request = (HttpWebRequest)WebRequest.Create(m_wiki.ApiUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = UserAgent;
            request.CookieContainer = m_cookies;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            return request;
        }

        /// <summary>
        /// Writes parameters to a stream that will be written to the body of a POST request.
        /// </summary>
        private static void WriteParameters(IEnumerable<Tuple<string, string>> parameters, StreamWriter writer)
        {
            bool first = true;

            foreach (var parameter in parameters)
            {
                if (!first)
                    writer.Write('&');

                writer.Write(parameter.Item1);
                writer.Write('=');
                writer.Write(Uri.EscapeDataString(parameter.Item2));

                first = false;
            }
        }
    }
}
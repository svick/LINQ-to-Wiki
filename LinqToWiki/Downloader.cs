using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Linq;
using System.Linq;

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
        }

        public static string UserAgent { get; set; }

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
            parameters = new[] { Tuple.Create("format", "xml") }.Concat(parameters);

            var request = CreateRequest();

            using (var requestStream = request.GetRequestStream())
            using (var requestWriter = new StreamWriter(requestStream))
            {
                WriteParameters(parameters, requestWriter);
            }

            var response = request.GetResponse();

            return XDocument.Load(response.GetResponseStream());
        }

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
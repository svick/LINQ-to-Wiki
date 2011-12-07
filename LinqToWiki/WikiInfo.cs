using System;

namespace LinqToWiki
{
    /// <summary>
    /// Contains information about a certain wiki necessary to access its API.
    /// </summary>
    public class WikiInfo
    {
        public Uri BaseUrl { get; private set; }

        public Uri ApiUrl { get; private set; }

        public Downloader Downloader { get; set; }

        public WikiInfo(string baseUrl = null, string apiPath = null)
        {
            if (baseUrl == null)
                baseUrl = "en.wikipedia.org";

            if (!baseUrl.StartsWith("http"))
                baseUrl = "http://" + baseUrl;

            BaseUrl = new Uri(baseUrl);

            if (apiPath == null)
                apiPath = "/w/api.php";

            ApiUrl = new Uri(BaseUrl, apiPath);

            Downloader = new Downloader(this);
        }
    }
}
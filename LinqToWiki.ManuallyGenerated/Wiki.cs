namespace LinqToWiki
{
    public sealed class Wiki
    {
        public Wiki(string baseUri = null, string apiPath = null)
        {
            var info = new WikiInfo(baseUri, apiPath);

            Query = new QueryAction(info);
        }

        public QueryAction Query { get; private set; }
    }
}
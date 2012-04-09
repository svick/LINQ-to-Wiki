namespace LinqToWiki
{
    public sealed class Wiki
    {
        private readonly WikiInfo m_info;

        public Wiki(string baseUri = null, string apiPath = null)
        {
            m_info = new WikiInfo(baseUri, apiPath);

            Query = new QueryAction(m_info);
        }

        public QueryAction Query { get; private set; }

        public NamespaceInfo Namespaces
        {
            get { return m_info.Namespaces; }
        }
    }
}
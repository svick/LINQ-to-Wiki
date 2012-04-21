using LinqToWiki.Collections;
using LinqToWiki.Parameters;

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

        public LoginResult Login(string name, string password, string token = null)
        {
            var queryProcessor = new QueryProcessor<LoginResult>(
                m_info,
                new QueryTypeProperties<LoginResult>(
                    "login", "lg", null, null, new TupleList<string, string> { { "action", "login" } }, null,
                    LoginResult.Parse));

            var parameters = QueryParameters.Create<LoginResult>();

            if (name != null)
                parameters = parameters.AddSingleValue("name", name);

            if (password != null)
                parameters = parameters.AddSingleValue("password", password);

            if (token != null)
                parameters = parameters.AddSingleValue("token", token);

            return queryProcessor.ExecuteSingle(parameters);
        }
    }
}
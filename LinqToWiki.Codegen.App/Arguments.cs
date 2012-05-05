namespace LinqToWiki.Codegen.App
{
    class Arguments
    {
        private readonly ArgumentsCollection m_argumentsCollection;

        private Arguments(ArgumentsCollection argumentsCollection)
        {
            m_argumentsCollection = argumentsCollection;
        }

        public static Arguments Parse(string[] args)
        {
            var argumentsCollection = ArgumentsCollection.Parse(args);
            return new Arguments(argumentsCollection);
        }

        public string Url
        {
            get { return m_argumentsCollection[0]; }
        }

        public string Namespace
        {
            get { return m_argumentsCollection[1] ?? "LinqToWiki.Generated"; }
        }

        public string OutputName
        {
            get { return m_argumentsCollection[2] ?? Namespace; }
        }

        public string Directory
        {
            get { return m_argumentsCollection['d'] ?? string.Empty; }
        }

        public string PropsFile
        {
            get { return m_argumentsCollection['p']; }
        }
    }
}
namespace LinqToWiki
{
    public sealed class Wiki
    {
        public Wiki()
        {
            Query = new QueryAction(this);
        }

        public QueryAction Query { get; private set; }
    }
}
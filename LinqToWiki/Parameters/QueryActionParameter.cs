namespace LinqToWiki.Parameters
{
    public enum QueryAction
    {
        Query
    }

    public sealed class QueryActionParameter : QueryParameter
    {
        public QueryAction Action { get; private set; }

        public QueryActionParameter(QueryAction action)
            : base(null)
        {
            Action = action;
        }

        public QueryTypeParameter AddType(QueryType queryType)
        {
            return new QueryTypeParameter(this, queryType);
        }
    }
}
namespace LinqToWiki.Parameters
{
    public abstract class QueryParameter
    {
        protected QueryParameter(QueryParameter previous)
        {
            Previous = previous;
        }

        public QueryParameter Previous { get; private set; }

        public static QueryActionParameter WithAction(QueryAction action)
        {
            return new QueryActionParameter(action);
        }

        public NameValuesParameter AddSingleValue(string name, string value)
        {
            return new NameValuesParameter(this, name, value);
        }
    }
}
using System;

namespace LinqToWiki.Parameters
{
    public enum QueryType
    {
        CategoryMembers
    }

    public sealed class QueryTypeParameter : QueryParameter
    {
        public QueryType QueryType { get; private set; }

        public QueryTypeParameter(QueryActionParameter previous, QueryType queryType)
            : base(previous)
        {
            if (previous.Action != QueryAction.Query)
                throw new ArgumentException();

            QueryType = queryType;
        }
    }
}
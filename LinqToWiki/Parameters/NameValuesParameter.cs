using System.Collections.Generic;

namespace LinqToWiki.Parameters
{
    public sealed class NameValuesParameter : QueryParameter
    {
        public string Name { get; private set; }

        public IEnumerable<string> Values { get; private set; }

        public NameValuesParameter(QueryParameter previous, string name, string value)
            : base(previous)
        {
            Name = name;
            Values = new[] { value };
        }

        public NameValuesParameter(QueryParameter previous, string name, IEnumerable<string> values)
            : base(previous)
        {
            Name = name;
            Values = values;
        }
    }
}
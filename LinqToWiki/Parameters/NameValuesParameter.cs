using System.Collections.Generic;

namespace LinqToWiki.Parameters
{
    /// <summary>
    /// Node of a linked list of name-values pairs, that represent general query parameters.
    /// </summary>
    public sealed class NameValuesParameter
    {
        /// <summary>
        /// Previous node in the list, or <c>null</c>, if this is the last one.
        /// </summary>
        public NameValuesParameter Previous { get; private set; }

        /// <summary>
        /// Name of the this parameter.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Values of this parameter.
        /// </summary>
        public IEnumerable<string> Values { get; private set; }

        public NameValuesParameter(NameValuesParameter previous, string name, string value)
        {
            Previous = previous;
            Name = name;
            Values = new[] { value };
        }

        public NameValuesParameter(NameValuesParameter previous, string name, IEnumerable<string> values)
        {
            Previous = previous;
            Name = name;
            Values = values;
        }
    }
}
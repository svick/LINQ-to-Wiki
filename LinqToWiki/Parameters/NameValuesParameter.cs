using System.Collections;
using System.Collections.Generic;

namespace LinqToWiki.Parameters
{
    /// <summary>
    /// Node of a linked list of name-values pairs, that represent general query parameters.
    /// Also acts as a collection of parameters from the linked list ending with the current one.
    /// </summary>
    public sealed class NameValuesParameter : IEnumerable<NameValuesParameter>
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

        public IEnumerator<NameValuesParameter> GetEnumerator()
        {
            var stack = new Stack<NameValuesParameter>();

            for (NameValuesParameter current = this; current != null; current = current.Previous)
                stack.Push(current);

            return stack.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return string.Format("{0}={1}", Name, JoinValues(Values));
        }

        public static string JoinValues(IEnumerable<string> values)
        {
            //TODO: escaping
            return string.Join("|", values);
        }
    }
}
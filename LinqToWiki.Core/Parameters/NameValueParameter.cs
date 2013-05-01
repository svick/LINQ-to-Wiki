using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace LinqToWiki.Parameters
{
    /// <summary>
    /// Node of a linked list of name-value pairs, that represent general query parameters.
    /// Also acts as a collection of parameters from the linked list ending with the current one.
    /// </summary>
    public class NameValueParameter : IEnumerable<NameValueParameter>
    {
        /// <summary>
        /// Previous node in the list, or <c>null</c>, if this is the last one.
        /// </summary>
        public NameValueParameter Previous { get; private set; }

        /// <summary>
        /// Name of the this parameter.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Value of this parameter.
        /// Can represent multiple actual values, separated by <c>|</c>.
        /// </summary>
        public string Value { get; private set; }

        public NameValueParameter(NameValueParameter previous, string name, string value)
        {
            Previous = previous;
            Name = name;
            Value = value;
        }

        public IEnumerator<NameValueParameter> GetEnumerator()
        {
            var stack = new Stack<NameValueParameter>();

            for (NameValueParameter current = this; current != null; current = current.Previous)
                stack.Push(current);

            return stack.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return string.Format("{0}={1}", Name, Value);
        }
    }

    public class NameFileParameter : NameValueParameter
    {
        public Stream File { get; private set; }

        public NameFileParameter(NameValueParameter previous, string name, Stream file)
            : base(previous, name, null)
        {
            File = file;
        }

        public override string ToString()
        {
            return string.Format("{0}=<file>", Name);
        }
    }
}
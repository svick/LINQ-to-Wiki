using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LinqToWiki.Codegen.ModuleInfo
{
    /// <summary>
    /// Parameter or property type whose value can be from a fixed set of strings.
    /// Is represented as <see cref="LinqToWiki.Internals.StringValue"/> in generated code.
    /// </summary>
    class EnumParameterType : ParameterType
    {
        /// <summary>
        /// The set of possible values.
        /// </summary>
        public IEnumerable<string> Values { get; private set; }

        /// <summary>
        /// Creates the type based on a <c>type</c> XML element.
        /// </summary>
        public EnumParameterType(XElement element)
        {
            Values = element.Elements().Select(e => (string)e).ToArray();
        }

        public override bool Equals(ParameterType other)
        {
            var otherEnum = other as EnumParameterType;

            if (otherEnum == null)
                return false;

            return this.Values.SequenceEqual(otherEnum.Values);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            return Equals(obj as ParameterType);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (var value in Values)
                    hash = hash * 23 + value.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format("[{0}]", string.Join(", ", Values));
        }
    }
}
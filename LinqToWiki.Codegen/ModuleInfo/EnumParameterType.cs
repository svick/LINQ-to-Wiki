using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LinqToWiki.Codegen.ModuleInfo
{
    class EnumParameterType : ParameterType
    {
        public IEnumerable<string> Values { get; private set; }

        public EnumParameterType(XElement element)
        {
            Values = element.Elements().Select(e => (string)e).ToArray();
        }

        public override bool Equals(ParameterType other)
        {
            var otherEnum = other as EnumParameterType;

            if (otherEnum == null)
                return false;

            // TODO: ignore order

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
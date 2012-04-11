using System;
using System.Xml.Linq;

namespace LinqToWiki.Codegen.ModuleInfo
{
    public class Property : IEquatable<Property>
    {
        public string Name { get; private set; }

        public ParameterType Type { get; private set; }

        public static Property Parse(XElement element)
        {
            return new Property
                   {
                       Name = (string)element.Attribute("name"),
                       Type = ParameterType.Parse(element)
                   };
        }

        public override string ToString()
        {
            return string.Format("Name: {0}, Type: {1}", Name, Type);
        }

        public bool Equals(Property other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(other.Name, Name) && Equals(other.Type, Type);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(Property))
                return false;
            return Equals((Property)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name.GetHashCode() * 397) ^ Type.GetHashCode();
            }
        }
    }
}
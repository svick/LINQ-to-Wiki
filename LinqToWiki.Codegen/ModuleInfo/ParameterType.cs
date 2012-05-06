using System;
using System.Xml.Linq;

namespace LinqToWiki.Codegen.ModuleInfo
{
    /// <summary>
    /// Base type for classes that represent the type of parameter or property in the API.
    /// </summary>
    public abstract class ParameterType : IEquatable<ParameterType>
    {
        public static ParameterType Parse(XElement element)
        {
            var typeAttribute = element.Attribute("type");
            if (typeAttribute != null)
                return new SimpleParameterType((string)typeAttribute);

            var typeElement = element.Element("type");
            return new EnumParameterType(typeElement);
        }

        public abstract bool Equals(ParameterType other);
    }
}
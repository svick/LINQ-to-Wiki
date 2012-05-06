using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LinqToWiki.Codegen.ModuleInfo
{
    /// <summary>
    /// Group of properties of a module that can be accessed by a common <c>prop</c> parameter.
    /// </summary>
    public class PropertyGroup
    {
        /// <summary>
        /// Name of the group, is the same as a value of the <c>prop</c> parameter.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Properties in this group.
        /// </summary>
        public IEnumerable<Property> Properties { get; internal set; }

        /// <summary>
        /// Parses the <c>prop</c> element.
        /// </summary>
        public static PropertyGroup Parse(XElement element)
        {
            return new PropertyGroup
                   {
                       Name = (string)element.Attribute("name"),
                       Properties = element.Element("properties").Elements().Select(Property.Parse).ToArray()
                   };
        }
    }
}
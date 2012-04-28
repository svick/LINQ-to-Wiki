using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LinqToWiki.Codegen.ModuleInfo
{
    public class PropertyGroup
    {
        public string Name { get; internal set; }

        public IEnumerable<Property> Properties { get; internal set; }

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
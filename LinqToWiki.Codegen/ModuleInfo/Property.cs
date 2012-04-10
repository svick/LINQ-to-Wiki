using System.Xml.Linq;

namespace LinqToWiki.Codegen.ModuleInfo
{
    public class Property
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
    }
}
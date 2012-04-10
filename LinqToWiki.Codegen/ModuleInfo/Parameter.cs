using System.Xml.Linq;

namespace LinqToWiki.Codegen.ModuleInfo
{
    public class Parameter
    {
        private Parameter()
        {}

        public string Name { get; private set; }

        public string Description { get; private set; }

        public ParameterType Type { get; set; }

        public static Parameter Parse(XElement element)
        {
            return new Parameter
                   {
                       Name = (string)element.Attribute("name"),
                       Description = (string)element.Attribute("description"),
                       Type = ParameterType.Parse(element)
                   };
        }

        public override string ToString()
        {
            return string.Format("Name: {0}, Type: {1}, Description: {2}", Name, Type, Description);
        }
    }
}
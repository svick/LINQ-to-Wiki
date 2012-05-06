using System.Xml.Linq;

namespace LinqToWiki.Codegen.ModuleInfo
{
    /// <summary>
    /// Parameter of a module.
    /// </summary>
    public class Parameter
    {
        private Parameter()
        {}

        /// <summary>
        /// Name of the parameter.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Description of the parameter.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Type of the parameter.
        /// </summary>
        public ParameterType Type { get; private set; }

        /// <summary>
        /// Is this parameter required?
        /// </summary>
        public bool Required { get; private set; }

        /// <summary>
        /// Does this parameter accept mutiple values of the given type?
        /// </summary>
        public bool Multi { get; private set; }

        /// <summary>
        /// Is this parameter deprecated?
        /// </summary>
        public bool Deprecated { get; private set; }

        /// <summary>
        /// Creates <see cref="Parameter"/> based on a <c>param</c> XML element.
        /// </summary>
        public static Parameter Parse(XElement element)
        {
            return new Parameter
                   {
                       Name = (string)element.Attribute("name"),
                       Description = (string)element.Attribute("description"),
                       Type = ParameterType.Parse(element),
                       Required = element.Attribute("required") != null,
                       Multi = element.Attribute("multi") != null,
                       Deprecated = element.Attribute("deprecated") != null
                   };
        }

        public override string ToString()
        {
            return string.Format("Name: {0}, Type: {1}, Description: {2}", Name, Type, Description);
        }
    }
}
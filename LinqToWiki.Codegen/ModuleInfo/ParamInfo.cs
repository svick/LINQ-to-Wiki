using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LinqToWiki.Codegen.ModuleInfo
{
    /// <summary>
    /// Contains information about modules and query modules.
    /// </summary>
    public class ParamInfo
    {
        /// <summary>
        /// Non-query modules
        /// </summary>
        public IEnumerable<Module> Modules { get; private set; }

        /// <summary>
        /// Query modules
        /// </summary>
        public IEnumerable<Module> QueryModules { get; private set; }

        /// <summary>
        /// Parses the <c>paraminfo</c> element.
        /// </summary>
        public static ParamInfo Parse(XElement element, Dictionary<string, XElement> propsDefaults)
        {
            var result = new ParamInfo();

            var modules = element.Element("modules");
            if (modules != null)
                result.Modules = modules.Elements().Select(e => Module.Parse(e, propsDefaults)).ToArray();

            var queryModules = element.Element("querymodules");
            if (queryModules != null)
                result.QueryModules = queryModules.Elements().Select(e => Module.Parse(e, propsDefaults)).ToArray();

            return result;
        }
    }
}
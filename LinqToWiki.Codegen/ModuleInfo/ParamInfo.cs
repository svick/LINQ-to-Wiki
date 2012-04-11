using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LinqToWiki.Codegen.ModuleInfo
{
    public class ParamInfo
    {
        public IEnumerable<Module> Modules { get; private set; }

        public IEnumerable<Module> QueryModules { get; private set; }

        public static ParamInfo Parse(XElement element)
        {
            var result = new ParamInfo();

            var modules = element.Element("modules");
            if (modules != null)
                result.Modules = modules.Elements().Select(Module.Parse).ToArray();

            var queryModules = element.Element("querymodules");
            if (queryModules != null)
                result.QueryModules = queryModules.Elements().Select(Module.Parse).ToArray();

            return result;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LinqToWiki.Collections;
using LinqToWiki.Internals;
using LinqToWiki.Parameters;

namespace LinqToWiki.Codegen.ModuleInfo
{
    class ModulesSource
    {
        private readonly QueryProcessor<ParamInfo> m_processor;

        private string[] m_moduleNames;
        private string[] m_queryModuleNames;

        public ModulesSource(WikiInfo wiki, string propsDefaultsPath = null)
        {
            Dictionary<string, XElement> propsDefaults = null;
            if (propsDefaultsPath != null)
                propsDefaults =
                    XDocument.Load(propsDefaultsPath).Root.Elements().ToDictionary(e => (string)e.Attribute("name"));

            m_processor = new QueryProcessor<ParamInfo>(
                wiki,
                new QueryTypeProperties<ParamInfo>(
                    "paraminfo", "", null, null,
                    new TupleList<string, string> { { "action", "paraminfo" } },
                    null,
                    e => ParamInfo.Parse(e, propsDefaults)));
        }

        private void RetrieveModuleNames()
        {
            var module = m_processor
                .ExecuteSingle(QueryParameters.Create<ParamInfo>().AddSingleValue("modules", "paraminfo"))
                .Modules.Single();

            m_moduleNames = ((EnumParameterType)module.Parameters.Single(p => p.Name == "modules").Type).Values.ToArray();
            m_queryModuleNames = ((EnumParameterType)module.Parameters.Single(p => p.Name == "querymodules").Type).Values.ToArray();
        }

        public IEnumerable<string> GetAllQueryModuleNames()
        {
            if (m_queryModuleNames == null)
                RetrieveModuleNames();

            return m_queryModuleNames;
        }

        public IEnumerable<string> GetAllModuleNames()
        {
            if (m_moduleNames == null)
                RetrieveModuleNames();

            return m_moduleNames;
        }

        public IEnumerable<Module> GetQueryModules(IEnumerable<string> moduleNames)
        {
            return m_processor
                .ExecuteSingle(QueryParameters.Create<ParamInfo>().AddMultipleValues("querymodules", moduleNames))
                .QueryModules;
        }

        public IEnumerable<Module> GetModules(IEnumerable<string> moduleNames)
        {
            return m_processor
                .ExecuteSingle(QueryParameters.Create<ParamInfo>().AddMultipleValues("modules", moduleNames))
                .Modules;
        }
    }
}
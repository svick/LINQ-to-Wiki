using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LinqToWiki.Collections;
using LinqToWiki.Internals;
using LinqToWiki.Parameters;

namespace LinqToWiki.Codegen.ModuleInfo
{
    /// <summary>
    /// Class that can be used to retrieve information about API modules available on a wiki.
    /// </summary>
    public class ModulesSource
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

        private static string[] GetParameterValues(IReadOnlyList<Module> modules, string moduleName, string parameterName)
        {
            var module = modules.Single(m => m.Name == moduleName);
            var parameter = module.Parameters.Single(p => p.Name == parameterName);
            return ((EnumParameterType)parameter.Type).Values.ToArray();
        }

        /// <summary>
        /// Loads module names from the wiki.
        /// </summary>
        private void RetrieveModuleNames()
        {
            var modules = m_processor.ExecuteSingle(
                QueryParameters.Create<ParamInfo>().AddMultipleValues("modules", new[] { "main", "paraminfo" }))
                .Modules.ToList();

            m_moduleNames = GetParameterValues(modules, "main", "action");
            m_queryModuleNames = GetParameterValues(modules, "paraminfo", "querymodules");
        }

        /// <summary>
        /// Returns the names of all query modules.
        /// </summary>
        public IEnumerable<string> GetAllQueryModuleNames()
        {
            if (m_queryModuleNames == null)
                RetrieveModuleNames();

            return m_queryModuleNames;
        }

        /// <summary>
        /// Returns the names of all non-query modules.
        /// </summary>
        public IEnumerable<string> GetAllModuleNames()
        {
            if (m_moduleNames == null)
                RetrieveModuleNames();

            return m_moduleNames;
        }

        /// <summary>
        /// Returns information about the given query modules.
        /// </summary>
        public IEnumerable<Module> GetQueryModules(IEnumerable<string> moduleNames)
        {
            return GetModulesInternal(moduleNames, "querymodules", info => info.QueryModules);
        }

        /// <summary>
        /// Returns information about the given non-query modules.
        /// </summary>
        public IEnumerable<Module> GetModules(IEnumerable<string> moduleNames)
        {
            return GetModulesInternal(moduleNames, "modules", info => info.Modules);
        }

        /// <summary>
        /// Returns information about the given modules
        /// (query or non-query, based on the <see cref="modulesSelector"/> and <see cref="parameterName"/>).
        /// </summary>
        private IEnumerable<Module> GetModulesInternal(
            IEnumerable<string> moduleNames, string parameterName, Func<ParamInfo, IEnumerable<Module>> modulesSelector)
        {
            const int pageSize = 50;
            var moduleNamesArray = moduleNames.ToArray();

            for (int i = 0; i < moduleNamesArray.Length; i += pageSize)
            {
                var result = m_processor
                    .ExecuteSingle(
                        QueryParameters.Create<ParamInfo>().AddMultipleValues(
                            parameterName, moduleNames.Skip(i).Take(pageSize)));

                foreach (var module in modulesSelector(result))
                    yield return module;
            }
        }
    }
}
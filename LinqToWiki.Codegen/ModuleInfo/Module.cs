using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LinqToWiki.Internals;

namespace LinqToWiki.Codegen.ModuleInfo
{
    /// <summary>
    /// Contains information about a single module (or query module) returned by the <c>paraminfo</c> module.
    /// </summary>
    public class Module
    {
        /// <summary>
        /// Description of the module
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Prefix used by parameters to this module.
        /// </summary>
        public string Prefix { get; private set; }

        /// <summary>
        /// Can this module be used as a parameter?
        /// </summary>
        public bool Generator { get; private set; }

        /// <summary>
        /// Name of the module
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Type of the query module.
        /// Is <c>null</c> for non-query modules.
        /// </summary>
        public QueryType? QueryType { get; private set; }

        /// <summary>
        /// Does this module return a list of results?
        /// </summary>
        public bool ListResult { get; private set; }

        /// <summary>
        /// Parameters accepted by the module
        /// </summary>
        public IEnumerable<Parameter> Parameters { get; private set; }

        /// <summary>
        /// Property groups that can be returned by the module
        /// </summary>
        public IEnumerable<PropertyGroup> PropertyGroups { get; private set; }

        /// <summary>
        /// Parses the <c>module</c> XML element from API response.
        /// Can also use information about <see cref="ListResult"/> (and <see cref="ListResult"/>)
        /// from another source, if the wiki doesn't supply them.
        /// </summary>
        public static Module Parse(XElement element, Dictionary<string, XElement> propsDefaults)
        {
            XElement propsDefault = null;
            if (propsDefaults != null)
                propsDefaults.TryGetValue((string)element.Attribute("name"), out propsDefault);

            var propsElement = element.Element("props") ??
                               (propsDefault == null ? null : propsDefault.Element("props"));

            return
                new Module
                {
                    Description = (string)element.Attribute("description"),
                    Prefix = (string)element.Attribute("prefix"),
                    Generator = element.Attribute("generator") != null,
                    Name = (string)element.Attribute("name"),
                    QueryType =
                        element.Attribute("querytype") == null
                            ? null
                            : (QueryType?)Enum.Parse(typeof(QueryType), (string)element.Attribute("querytype"), true),
                    ListResult =
                        element.Attribute("listresult") != null
                        || (propsDefault != null && propsDefault.Attribute("listresult") != null),
                    Parameters = element.Element("parameters").Elements().Select(Parameter.Parse).ToArray(),
                    PropertyGroups =
                        propsElement == null
                            ? null
                            : propsElement.Elements().Select(PropertyGroup.Parse).ToArray()
                };
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
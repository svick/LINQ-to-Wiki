using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LinqToWiki.Internals;

namespace LinqToWiki.Codegen.ModuleInfo
{
    public class Module
    {
        public string ClassName { get; private set; }

        public string Description { get; private set; }

        public string Prefix { get; private set; }

        public bool Generator { get; private set; }

        public string Name { get; private set; }

        public QueryType? QueryType { get; private set; }

        public IEnumerable<Parameter> Parameters { get; private set; }

        public IEnumerable<PropertyGroup> PropertyGroups { get; private set; }

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
                    ClassName = (string)element.Attribute("classname"),
                    Description = (string)element.Attribute("description"),
                    Prefix = (string)element.Attribute("prefix"),
                    Generator = element.Attribute("generator") != null,
                    Name = (string)element.Attribute("name"),
                    QueryType =
                        element.Attribute("querytype") == null
                            ? null
                            : (QueryType?)Enum.Parse(typeof(QueryType), (string)element.Attribute("querytype"), true),
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
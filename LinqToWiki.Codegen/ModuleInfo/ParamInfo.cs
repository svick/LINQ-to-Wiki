using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LinqToWiki.Codegen.ModuleInfo
{
    class ParamInfo
    {
        public string Description { get; private set; }

        public string Prefix { get; private set; }

        public bool Generator { get; private set; }

        public string Name { get; private set; }

        public QueryType? QueryType { get; private set; }

        public IEnumerable<Parameter> Parameters { get; set; }

        public IEnumerable<PropertyGroup> PropertyGroups { get; private set; }

        public static ParamInfo Parse(XElement element)
        {
            return
                new ParamInfo
                {
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
                        element.Element("props") == null
                            ? null
                            : element.Element("props").Elements().Select(PropertyGroup.Parse).ToArray()
                };
        }
    }
}
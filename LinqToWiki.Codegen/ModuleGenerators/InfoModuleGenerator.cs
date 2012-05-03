using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Collections;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen.ModuleGenerators
{
    class InfoModuleGenerator : SinglePropModuleGenerator
    {
        public InfoModuleGenerator(Wiki wiki)
            : base(wiki)
        {}

        private static readonly PropertyGroup SpecialProperties =
            new PropertyGroup
            {
                Name = string.Empty,
                Properties =
                    new[]
                    {
                        new Property { Name = "pageid", Type = new SimpleParameterType("integer"), Nullable = true },
                        new Property { Name = "ns", Type = new SimpleParameterType("namespace"), Nullable = true },
                        new Property { Name = "title", Type = new SimpleParameterType("string"), Nullable = true },
                        new Property { Name = "missing", Type = new SimpleParameterType("boolean") },
                        new Property { Name = "invalid", Type = new SimpleParameterType("boolean") },
                        new Property { Name = "special", Type = new SimpleParameterType("boolean") }
                    }
            };

        protected override IEnumerable<PropertyGroup> GetPropertyGroups(Module module)
        {
            return new[] { SpecialProperties }.Concat(base.GetPropertyGroups(module));
        }

        protected override ClassDeclarationSyntax GenerateResultClass(IEnumerable<PropertyGroup> propertyGroups)
        {
            return GenerateClassForProperties(ResultClassName, propertyGroups.SelectMany(g => g.Properties));
        }
    }
}
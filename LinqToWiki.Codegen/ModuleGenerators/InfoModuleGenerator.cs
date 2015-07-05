using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Codegen.ModuleInfo;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LinqToWiki.Codegen.ModuleGenerators
{
    /// <summary>
    /// Generates code for the <c>info</c> query module.
    /// </summary>
    class InfoModuleGenerator : SinglePropModuleGenerator
    {
        public InfoModuleGenerator(Wiki wiki)
            : base(wiki)
        {}

        /// <summary>
        /// Properties that are always present and don't actually require the <c>info</c> module.
        /// </summary>
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
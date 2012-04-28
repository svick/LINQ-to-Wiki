using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Collections;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen.ModuleGenerators
{
    public class InfoModuleGenerator : ModuleGenerator
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
                        new Property { Name = "pageid", Type = new SimpleParameterType("integer") },
                        new Property { Name = "ns", Type = new SimpleParameterType("namespace") },
                        new Property { Name = "title", Type = new SimpleParameterType("string") }
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

        protected override void GenerateMethod(Module module)
        {
            var propsField = CreatePropsField(GetPropertyGroups(module));

            var propertiesField = CreatePropertiesField(module, ResultClassName, propsField, null);

            var moduleProperty = CreateThrowingProperty(module);

            AddMembersToClass(Wiki.Names.Page, propsField, propertiesField, moduleProperty);
        }

        protected override IEnumerable<Tuple<string, string>> GetBaseParameters(Module module)
        {
            return new TupleList<string, string>();
        }

        private PropertyDeclarationSyntax CreateThrowingProperty(Module module)
        {
            var summary = SyntaxEx.DocumentationSummary(module.Description);

            return SyntaxEx.PropertyDeclaration(
                new[] { SyntaxKind.PublicKeyword }, GenerateMethodResultType(), ClassNameBase,
                new StatementSyntax[] { SyntaxEx.Throw(SyntaxEx.ObjectCreation("NotSupportedException")) })
                .WithLeadingTrivia(Syntax.Trivia(SyntaxEx.DocumentationComment(summary)));
        }
    }
}
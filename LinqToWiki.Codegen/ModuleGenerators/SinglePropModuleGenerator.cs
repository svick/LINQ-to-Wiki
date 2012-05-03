using System;
using System.Collections.Generic;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Collections;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen.ModuleGenerators
{
    class SinglePropModuleGenerator : ModuleGenerator
    {
        public SinglePropModuleGenerator(Wiki wiki)
            : base(wiki)
        {}

        protected override void GenerateMethod(Module module)
        {
            var propsField = CreatePropsField(GetPropertyGroups(module));

            var propertiesField = CreatePropertiesField(module, ResultClassName, propsField, null);

            var moduleProperty = CreateProperty(module);

            AddMembersToClass(Wiki.Names.Page, propsField, propertiesField, moduleProperty);
        }

        private PropertyDeclarationSyntax CreateProperty(Module module)
        {
            var summary = SyntaxEx.DocumentationSummary(module.Description);

            return SyntaxEx.AutoPropertyDeclaration(
                new[] { SyntaxKind.PublicKeyword, SyntaxKind.AbstractKeyword }, GenerateMethodResultType(),
                ClassNameBase, isAbstarct: true, setModifier: SyntaxKind.PrivateKeyword)
                .WithLeadingTrivia(Syntax.Trivia(SyntaxEx.DocumentationComment(summary)));
        }

        protected override IEnumerable<Tuple<string, string>> GetBaseParameters(Module module)
        {
            return new TupleList<string, string>();
        }
    }
}
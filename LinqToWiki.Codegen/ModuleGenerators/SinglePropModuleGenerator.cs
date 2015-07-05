using System;
using System.Collections.Generic;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Collections;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LinqToWiki.Codegen.ModuleGenerators
{
    /// <summary>
    /// Generates code for <see cref="LinqToWiki.Internals.QueryType.Prop"/> query modules
    /// that return a single result (like <c>categoryinfo</c>).
    /// </summary>
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

        /// <summary>
        /// Creates the property that can used to access the information from the given module.
        /// </summary>
        private PropertyDeclarationSyntax CreateProperty(Module module)
        {
            var summary = SyntaxEx.DocumentationSummary(module.Description);

            return SyntaxEx.AutoPropertyDeclaration(
                new[] { SyntaxKind.PublicKeyword, SyntaxKind.AbstractKeyword }, GenerateMethodResultType(),
                ClassNameBase, isAbstract: true, setModifier: SyntaxKind.PrivateKeyword)
                .WithLeadingTrivia(SyntaxFactory.Trivia(SyntaxEx.DocumentationComment(summary)));
        }

        protected override IEnumerable<Tuple<string, string>> GetBaseParameters(Module module)
        {
            return new TupleList<string, string>();
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Codegen.ModuleInfo;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen.ModuleGenerators
{
    public class InfoModuleGenerator : ModuleGenerator
    {
        public InfoModuleGenerator(Wiki wiki)
            : base(wiki)
        {}

        protected override ClassDeclarationSyntax GenerateResultClass(IEnumerable<PropertyGroup> propertyGroups)
        {
            return GenerateClassForProperties(ResultClassName, propertyGroups.SelectMany(g => g.Properties));
        }

        protected override void GenerateMethod(Module module)
        {
            var containingFile = Wiki.Files[Wiki.Names.Page];
            var containingClass = containingFile.SingleDescendant<ClassDeclarationSyntax>();

            var propertiesField = CreatePropertiesField(module, ResultClassName, null, null);

            var moduleProperty = CreateThrowingProperty(module);

            Wiki.Files[Wiki.Names.Page] = containingFile.ReplaceNode(
                containingClass, containingClass.WithAdditionalMembers(propertiesField, moduleProperty));
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
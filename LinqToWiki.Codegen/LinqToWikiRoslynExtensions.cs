using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen
{
    /// <summary>
    /// Contains LinqToWiki-specific Roslyn extension methods.
    /// </summary>
    static class LinqToWikiRoslynExtensions
    {
        /// <summary>
        /// Returns the given class with a private parameterless constructor added.
        /// </summary>
        public static ClassDeclarationSyntax AddPrivateConstructor(this ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.AddMembers(
                SyntaxEx.ConstructorDeclaration(
                    new[] { SyntaxKind.PrivateKeyword }, classDeclaration.Identifier.ValueText));
        }
    }
}
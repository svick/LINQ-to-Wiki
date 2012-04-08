using System;
using System.Collections.Generic;
using Roslyn.Compilers.CSharp;
using System.Linq;

namespace LinqToWiki.Codegen
{
    public static class RoslynExtensions
    {
         public static string GetName(this FieldDeclarationSyntax fieldDeclaration)
         {
             return fieldDeclaration.Declaration.Variables.Single().Identifier.ValueText;
         }

        public static string GetName(this PropertyDeclarationSyntax propertyDeclaration)
        {
            return propertyDeclaration.Identifier.ValueText;
        }

        public static string GetName(this ParameterSyntax parameter)
        {
            return parameter.Identifier.ValueText;
        }

        public static SeparatedSyntaxList<T> AsSeparatedList<T>(this SyntaxList<T> list, SyntaxKind separator) where T : SyntaxNode
        {
            return Syntax.SeparatedList(list, Enumerable.Repeat(Syntax.Token(separator), Math.Max(list.Count - 1, 0)));
        }
    }

    public static class SyntaxEx
    {
        public static FieldDeclarationSyntax FieldDeclaration(IEnumerable<SyntaxKind> modifiers, string typeName, string fieldName)
        {
            return Syntax.FieldDeclaration(
                modifiers: TokenList(modifiers),
                declaration:
                    Syntax.VariableDeclaration(
                        Syntax.ParseTypeName(typeName),
                        Syntax.SeparatedList(Syntax.VariableDeclarator(Syntax.Identifier(fieldName)))));
        }

        public static FieldDeclarationSyntax FieldDeclaration(IEnumerable<SyntaxKind> modifiers, string typeName, string fieldName, ExpressionSyntax initializer)
        {
            return Syntax.FieldDeclaration(
                modifiers: TokenList(modifiers),
                declaration:
                    Syntax.VariableDeclaration(
                        Syntax.ParseTypeName(typeName),
                        Syntax.SeparatedList(
                            Syntax.VariableDeclarator(
                                Syntax.Identifier(fieldName),
                                initializerOpt: Syntax.EqualsValueClause(value: initializer)))));
        }

        private static SyntaxTokenList TokenList(IEnumerable<SyntaxKind> modifiers)
        {
            return Syntax.TokenList(
                modifiers.Select(Syntax.Token));
        }

        private static SyntaxTokenList TokenList(SyntaxKind? modifier)
        {
            return modifier == null ? Syntax.TokenList() : Syntax.TokenList(Syntax.Token(modifier.Value));
        }

        public static ClassDeclarationSyntax ClassDeclaration(string className, params MemberDeclarationSyntax[] members)
        {
            return Syntax.ClassDeclaration(
                identifier: Syntax.Identifier(className),
                members: Syntax.List(members));
        }

        public static ConstructorDeclarationSyntax ConstructorDeclaration(
            IEnumerable<SyntaxKind> modifiers, string className, IEnumerable<ParameterSyntax> parameters,
            IEnumerable<StatementSyntax> statements, ConstructorInitializerSyntax constructorInitializer = null)
        {
            return Syntax.ConstructorDeclaration(
                modifiers: TokenList(modifiers),
                identifier: Syntax.Identifier(className),
                parameterList:
                    Syntax.ParameterList(parameters: Syntax.List(parameters).AsSeparatedList(SyntaxKind.CommaToken)),
                bodyOpt: Syntax.Block(statements: Syntax.List(statements)),
                initializerOpt: constructorInitializer);
        }

        public static ConstructorInitializerSyntax ThisConstructorInitializer(params ExpressionSyntax[] arguments)
        {
            return Syntax.ConstructorInitializer(
                SyntaxKind.ThisConstructorInitializer,
                argumentList: Syntax.ArgumentList(
                    arguments: Syntax.List(arguments.Select(a => Syntax.Argument(expression: a)))
                        .AsSeparatedList(SyntaxKind.CommaToken)));
        }

        public static ParameterSyntax Parameter(string type, string name)
        {
            return Syntax.Parameter(typeOpt: Syntax.ParseTypeName(type), identifier: Syntax.Identifier(name));
        }

        public static ExpressionStatementSyntax Assignment(NamedNode left, NamedNode right)
        {
            return Assignment(Syntax.IdentifierName(left.Name), Syntax.IdentifierName(right.Name));
        }

        public static ExpressionStatementSyntax Assignment(NamedNode left, ExpressionSyntax right)
        {
            return Assignment(Syntax.IdentifierName(left.Name), right);
        }

        public static ExpressionStatementSyntax Assignment(ExpressionSyntax left, ExpressionSyntax right)
        {
            return
                Syntax.ExpressionStatement(
                    Syntax.BinaryExpression(SyntaxKind.AssignExpression, left: left, right: right));
        }

        public static LiteralExpressionSyntax Literal(string value)
        {
            // TODO: escaping
            return Syntax.LiteralExpression(SyntaxKind.StringLiteralExpression, Syntax.Literal('"' + value + '"', value));
        }

        public static LiteralExpressionSyntax NullLiteral()
        {
            return Syntax.LiteralExpression(SyntaxKind.NullLiteralExpression);
        }

        public static NamespaceDeclarationSyntax NamespaceDeclaration(string name, params MemberDeclarationSyntax[] members)
        {
            return Syntax.NamespaceDeclaration(name: Syntax.ParseName(name), members: Syntax.List(members));
        }

        public static CompilationUnitSyntax CompilationUnit(MemberDeclarationSyntax member, params string[] usings)
        {
            return
                Syntax.CompilationUnit(
                    usings: Syntax.List(usings.Select(u => Syntax.UsingDirective(name: Syntax.ParseName(u)))),
                    members: Syntax.List(member));
        }

        public static PropertyDeclarationSyntax AutoPropertyDeclaration(
            IEnumerable<SyntaxKind> modifiers, string typeName, string propertyName,
            SyntaxKind? setModifier = null, SyntaxKind? getModifier = null)
        {
            return Syntax.PropertyDeclaration(
                modifiers: TokenList(modifiers), type: Syntax.ParseTypeName(typeName),
                identifier: Syntax.Identifier(propertyName),
                accessorList: Syntax.AccessorList(
                    accessors: Syntax.List(
                        Syntax.AccessorDeclaration(
                            SyntaxKind.GetAccessorDeclaration,
                            modifiers: TokenList(getModifier),
                            semicolonTokenOpt: Syntax.Token(SyntaxKind.SemicolonToken)),
                        Syntax.AccessorDeclaration(
                            SyntaxKind.SetAccessorDeclaration,
                            modifiers: TokenList(setModifier),
                            semicolonTokenOpt: Syntax.Token(SyntaxKind.SemicolonToken)))));
        }

        public static ObjectCreationExpressionSyntax ObjectCreation(string typeName, params ExpressionSyntax[] arguments)
        {
            var argumentList =
                arguments.Length == 0
                    ? null
                    : Syntax.ArgumentList(
                        arguments: Syntax.List(
                            arguments.Select(a => Syntax.Argument(expression: a)))
                            .AsSeparatedList(SyntaxKind.CommaToken));

            return Syntax.ObjectCreationExpression(type: Syntax.ParseTypeName(typeName), argumentListOpt: argumentList);
        }
    }

    public class NamedNode
    {
        private readonly string m_name;

        private NamedNode(string name)
        {
            m_name = name;
        }

        public static implicit operator NamedNode(FieldDeclarationSyntax fieldDeclaration)
        {
            return new NamedNode(fieldDeclaration.GetName());
        }

        public static implicit operator NamedNode(PropertyDeclarationSyntax propertyDeclaration)
        {
            return new NamedNode(propertyDeclaration.GetName());
        }

        public static implicit operator NamedNode(ParameterSyntax parameter)
        {
            return new NamedNode(parameter.GetName());
        }

        public static implicit operator IdentifierNameSyntax(NamedNode namedNode)
        {
            return Syntax.IdentifierName(namedNode.Name);
        }

        public string Name
        {
            get
            {
                if (m_name == null)
                    throw new InvalidOperationException();
                return m_name;
            }
        }
    }
}
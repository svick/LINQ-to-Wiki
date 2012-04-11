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

        public static string GetName(this LocalDeclarationStatementSyntax localDeclarationStatement)
        {
            return localDeclarationStatement.Declaration.Variables.Single().Identifier.ValueText;
        }

        public static SyntaxList<T> ToSyntaxList<T>(this IEnumerable<T> nodes) where T : SyntaxNode
        {
            return Syntax.List(nodes);
        }

        public static SeparatedSyntaxList<T> ToSeparatedList<T>(
            this IEnumerable<T> nodes, SyntaxKind separator = SyntaxKind.CommaToken)
            where T : SyntaxNode
        {
            var nodesArray = nodes == null ? new T[0] : nodes.ToArray();
            return Syntax.SeparatedList(
                nodesArray, Enumerable.Repeat(Syntax.Token(separator), Math.Max(nodesArray.Length - 1, 0)));
        }

        public static ClassDeclarationSyntax Update(
            this ClassDeclarationSyntax classDeclaration, SyntaxList<MemberDeclarationSyntax> members)
        {
            return classDeclaration.Update(
                classDeclaration.Attributes, classDeclaration.Modifiers, classDeclaration.Keyword,
                classDeclaration.Identifier, classDeclaration.TypeParameterListOpt, classDeclaration.BaseListOpt,
                classDeclaration.ConstraintClauses, classDeclaration.OpenBraceToken, members,
                classDeclaration.CloseBraceToken, classDeclaration.SemicolonTokenOpt);
        }

        public static ClassDeclarationSyntax WithAdditionalMembers(
            this ClassDeclarationSyntax classDeclaration, params MemberDeclarationSyntax[] members)
        {
            return classDeclaration.WithAdditionalMembers((IEnumerable<MemberDeclarationSyntax>)members);
        }

        public static ClassDeclarationSyntax WithAdditionalMembers(
            this ClassDeclarationSyntax classDeclaration, IEnumerable<MemberDeclarationSyntax> members)
        {
            return classDeclaration.Update(classDeclaration.Members.Concat(members).ToSyntaxList());
        }

        public static NamespaceDeclarationSyntax Update(
            this NamespaceDeclarationSyntax namespaceDeclaration, SyntaxList<MemberDeclarationSyntax> members)
        {
            return namespaceDeclaration.Update(
                namespaceDeclaration.NamespaceKeyword, namespaceDeclaration.Name, namespaceDeclaration.OpenBraceToken,
                namespaceDeclaration.Externs, namespaceDeclaration.Usings, members, namespaceDeclaration.CloseBraceToken,
                namespaceDeclaration.SemicolonTokenOpt);
        }

        public static NamespaceDeclarationSyntax WithAdditionalMembers(
            this NamespaceDeclarationSyntax namespaceDeclaration, params MemberDeclarationSyntax[] members)
        {
            return namespaceDeclaration.Update(namespaceDeclaration.Members.Concat(members).ToSyntaxList());
        }

        public static T SingleDescendant<T>(this SyntaxNode node) where T : SyntaxNode
        {
            return node.DescendentNodes().OfType<T>().Single();
        }
    }

    public static class SyntaxEx
    {
        public static TypeSyntax ParseTypeName(string typeName)
        {
            if (typeName == "var")
                return Syntax.IdentifierName(Syntax.Token(SyntaxKind.VarKeyword));

            return Syntax.ParseTypeName(typeName);
        }

        public static FieldDeclarationSyntax FieldDeclaration(IEnumerable<SyntaxKind> modifiers, string typeName, string fieldName, ExpressionSyntax initializer = null)
        {
            return FieldDeclaration(modifiers, Syntax.ParseTypeName(typeName), fieldName, initializer);
        }

        public static FieldDeclarationSyntax FieldDeclaration(IEnumerable<SyntaxKind> modifiers, TypeSyntax type, string fieldName, ExpressionSyntax initializer = null)
        {
            return Syntax.FieldDeclaration(
                modifiers: TokenList(modifiers),
                declaration:
                    Syntax.VariableDeclaration(
                        type, Syntax.SeparatedList(
                            Syntax.VariableDeclarator(
                                Syntax.Identifier(fieldName),
                                initializerOpt:
                                    initializer == null ? null : Syntax.EqualsValueClause(value: initializer)))));
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
            return ClassDeclaration(className, (IEnumerable<MemberDeclarationSyntax>)members);
        }

        public static ClassDeclarationSyntax ClassDeclaration(string className, IEnumerable<MemberDeclarationSyntax> members)
        {
            return Syntax.ClassDeclaration(
                modifiers: TokenList(new[] {SyntaxKind.PublicKeyword, SyntaxKind.SealedKeyword }),
                identifier: Syntax.Identifier(className),
                members: members.ToSyntaxList());
        }

        public static EnumDeclarationSyntax EnumDeclaration(string enumName, IEnumerable<string> members)
        {
            return Syntax.EnumDeclaration(
                modifiers: TokenList(SyntaxKind.PublicKeyword),
                identifier: Syntax.Identifier(enumName),
                members:
                    members.Select(m => Syntax.EnumMemberDeclaration(identifier: Syntax.Identifier(m)))
                    .ToSeparatedList());
        }

        public static ConstructorDeclarationSyntax ConstructorDeclaration(
            IEnumerable<SyntaxKind> modifiers, string className, IEnumerable<ParameterSyntax> parameters = null,
            IEnumerable<StatementSyntax> statements = null, ConstructorInitializerSyntax constructorInitializer = null)
        {
            return Syntax.ConstructorDeclaration(
                modifiers: TokenList(modifiers),
                identifier: Syntax.Identifier(className),
                parameterList: Syntax.ParameterList(parameters: parameters.ToSeparatedList()),
                bodyOpt: Syntax.Block(statements: statements.ToSyntaxList()),
                initializerOpt: constructorInitializer);
        }

        public static ConstructorInitializerSyntax ThisConstructorInitializer(params ExpressionSyntax[] arguments)
        {
            return Syntax.ConstructorInitializer(
                SyntaxKind.ThisConstructorInitializer,
                argumentList: Syntax.ArgumentList(
                    arguments: arguments.Select(a => Syntax.Argument(expression: a)).ToSeparatedList()));
        }

        public static ParameterSyntax Parameter(string type, string name, ExpressionSyntax defaultValue = null)
        {
            return Syntax.Parameter(
                typeOpt: Syntax.ParseTypeName(type),
                identifier: Syntax.Identifier(name),
                defaultOpt: defaultValue == null ? null : Syntax.EqualsValueClause(value: defaultValue));
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

        public static LiteralExpressionSyntax Literal(bool value)
        {
            return Syntax.LiteralExpression(
                value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
        }

        public static LiteralExpressionSyntax NullLiteral()
        {
            return Syntax.LiteralExpression(SyntaxKind.NullLiteralExpression);
        }

        public static NamespaceDeclarationSyntax NamespaceDeclaration(string name, params MemberDeclarationSyntax[] members)
        {
            return Syntax.NamespaceDeclaration(name: Syntax.ParseName(name), members: Syntax.List(members));
        }

        public static CompilationUnitSyntax CompilationUnit(NamespaceDeclarationSyntax member, params string[] usings)
        {
            return
                Syntax.CompilationUnit(
                    usings: usings.Select(u => Syntax.UsingDirective(name: Syntax.ParseName(u))).ToSyntaxList(),
                    members: Syntax.List<MemberDeclarationSyntax>(member));
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

        public static MethodDeclarationSyntax MethodDeclaration(
            IEnumerable<SyntaxKind> modifiers, string returnTypeName, string methodName,
            IEnumerable<ParameterSyntax> parameters, IEnumerable<StatementSyntax> statements)
        {
            return MethodDeclaration(
                modifiers, Syntax.ParseTypeName(returnTypeName), methodName, parameters, statements);
        }

        public static MethodDeclarationSyntax MethodDeclaration(
            IEnumerable<SyntaxKind> modifiers, TypeSyntax returnType, string methodName,
            IEnumerable<ParameterSyntax> parameters, params StatementSyntax[] statements)
        {
            return MethodDeclaration(
                modifiers, returnType, methodName, parameters, (IEnumerable<StatementSyntax>)statements);
        }

        public static MethodDeclarationSyntax MethodDeclaration(
            IEnumerable<SyntaxKind> modifiers, TypeSyntax returnType, string methodName,
            IEnumerable<ParameterSyntax> parameters, IEnumerable<StatementSyntax> statements)
        {
            return Syntax.MethodDeclaration(
                modifiers: TokenList(modifiers),
                returnType: returnType,
                identifier: Syntax.Identifier(methodName),
                parameterList: Syntax.ParameterList(parameters: parameters.ToSeparatedList()),
                bodyOpt: Syntax.Block(statements: statements.ToSyntaxList()));
        }

        public static ObjectCreationExpressionSyntax ObjectCreation(
            string typeName, IEnumerable<ExpressionSyntax> arguments,
            IEnumerable<IEnumerable<ExpressionSyntax>> initializers = null)
        {
            return ObjectCreation(Syntax.ParseTypeName(typeName), arguments, initializers);
        }

        public static ObjectCreationExpressionSyntax ObjectCreation(string typeName, params ExpressionSyntax[] arguments)
        {
            return ObjectCreation(typeName, (IEnumerable<ExpressionSyntax>)arguments);
        }

        public static ObjectCreationExpressionSyntax ObjectCreation(TypeSyntax type, params ExpressionSyntax[] arguments)
        {
            return ObjectCreation(type, (IEnumerable<ExpressionSyntax>)arguments);
        }

        public static ObjectCreationExpressionSyntax ObjectCreation(
            TypeSyntax type, IEnumerable<ExpressionSyntax> arguments, IEnumerable<ExpressionSyntax> initializers)
        {
            return ObjectCreation(type, arguments, initializers.Select(i => new[] { i }));
        }

        public static ObjectCreationExpressionSyntax ObjectCreation(
            TypeSyntax type, IEnumerable<ExpressionSyntax> arguments, IEnumerable<IEnumerable<ExpressionSyntax>> initializers = null)
        {
            var argumentsArray = arguments == null ? new ExpressionSyntax[0] : arguments.ToArray();
            var argumentList =
                argumentsArray.Length == 0
                    ? Syntax.ArgumentList()
                    : Syntax.ArgumentList(
                        arguments: argumentsArray.Select(a => Syntax.Argument(expression: a)).ToSeparatedList());

            InitializerExpressionSyntax initializer =
                initializers == null
                    ? null
                    : Syntax.InitializerExpression(expressions: initializers.Select(ToInitializer).ToSeparatedList());

            return Syntax.ObjectCreationExpression(
                type: type, argumentListOpt: argumentList, initializerOpt: initializer);
        }

        private static ExpressionSyntax ToInitializer(IEnumerable<ExpressionSyntax> expressions)
        {
            var expressionsArray = expressions.ToArray();

            if (expressionsArray.Length == 1)
                return expressionsArray[0];

            return Syntax.InitializerExpression(expressions: expressionsArray.ToSeparatedList());
        }

        public static ExpressionSyntax ArrayCreation(string typeName, IEnumerable<ExpressionSyntax> expressions)
        {
            var initializer = Syntax.InitializerExpression(expressions: expressions.ToSeparatedList());

            if (string.IsNullOrEmpty(typeName))
                return Syntax.ImplicitArrayCreationExpression(initializer: initializer);

            return Syntax.ArrayCreationExpression(
                type: Syntax.ArrayType(Syntax.ParseTypeName(typeName), Syntax.List(Syntax.ArrayRankSpecifier())),
                initializerOpt: initializer);
        }

        public static LocalDeclarationStatementSyntax LocalDeclaration(string type, string localName, ExpressionSyntax value)
        {
            return Syntax.LocalDeclarationStatement(
                declaration: Syntax.VariableDeclaration(
                    SyntaxEx.ParseTypeName(type),
                    Syntax.SeparatedList(
                        Syntax.VariableDeclarator(
                            Syntax.Identifier(localName), initializerOpt: Syntax.EqualsValueClause(value: value)))));
        }

        public static ReturnStatementSyntax Return(NamedNode namedNode)
        {
            return Syntax.ReturnStatement(expressionOpt: namedNode);
        }

        public static ReturnStatementSyntax Return(ExpressionSyntax expression)
        {
            return Syntax.ReturnStatement(expressionOpt: expression);
        }

        public static IfStatementSyntax If(
            ExpressionSyntax condition, StatementSyntax ifStatement, StatementSyntax elseStatement = null)
        {
            return Syntax.IfStatement(
                condition: condition,
                statement: ifStatement,
                elseOpt: elseStatement == null ? null : Syntax.ElseClause(statement: elseStatement));
        }

        public static InvocationExpressionSyntax Invocation(ExpressionSyntax expression, params ExpressionSyntax[] arguments)
        {
            return Invocation(expression, (IEnumerable<ExpressionSyntax>)arguments);
        }

        public static InvocationExpressionSyntax Invocation(ExpressionSyntax expression, IEnumerable<ExpressionSyntax> arguments)
        {
            return Syntax.InvocationExpression(
                expression,
                Syntax.ArgumentList(arguments: arguments.Select(a => Syntax.Argument(expression: a)).ToSeparatedList()));
        }

        public static ElementAccessExpressionSyntax ElementAccess(ExpressionSyntax expression, params ExpressionSyntax[] arguments)
        {
            return Syntax.ElementAccessExpression(
                expression,
                Syntax.BracketedArgumentList(
                    arguments: arguments.Select(a => Syntax.Argument(expression: a)).ToSeparatedList()));
        }

        public static MemberAccessExpressionSyntax MemberAccess(string name, string memberName)
        {
            return MemberAccess(Syntax.ParseExpression(name), memberName);
        }

        public static MemberAccessExpressionSyntax MemberAccess(NamedNode namedNode, string memberName)
        {
            return MemberAccess((ExpressionSyntax)namedNode, memberName);
        }

        public static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax expression, string memberName)
        {
            return MemberAccess(expression, (SimpleNameSyntax)Syntax.ParseName(memberName));
        }

        public static MemberAccessExpressionSyntax MemberAccess(string name, SimpleNameSyntax memberName)
        {
            return MemberAccess(Syntax.ParseExpression(name), memberName);
        }

        public static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax expression, SimpleNameSyntax memberName)
        {
            return Syntax.MemberAccessExpression(SyntaxKind.MemberAccessExpression, expression, name: memberName);
        }

        public static BinaryExpressionSyntax Equals(ExpressionSyntax left, ExpressionSyntax right)
        {
            return Syntax.BinaryExpression(SyntaxKind.EqualsExpression, left: left, right: right);
        }

        public static BinaryExpressionSyntax NotEquals(ExpressionSyntax left, ExpressionSyntax right)
        {
            return Syntax.BinaryExpression(SyntaxKind.NotEqualsExpression, left: left, right: right);
        }

        public static TypeOfExpressionSyntax TypeOf(string typeName)
        {
            return Syntax.TypeOfExpression(
                argumentList: Syntax.ArgumentList(
                    arguments: Syntax.SeparatedList(Syntax.Argument(expression: Syntax.ParseTypeName(typeName)))));
        }

        public static CastExpressionSyntax Cast(string typeName, ExpressionSyntax expression)
        {
            return Syntax.CastExpression(type: Syntax.ParseTypeName(typeName), expression: expression);
        }

        public static GenericNameSyntax GenericName(string name, params string[] typeArgumentNames)
        {
            return Syntax.GenericName(
                Syntax.Identifier(name),
                Syntax.TypeArgumentList(
                    arguments: typeArgumentNames.Select(Syntax.IdentifierName).ToSeparatedList<TypeSyntax>()));
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

        public static implicit operator NamedNode(LocalDeclarationStatementSyntax localDeclarationStatement)
        {
            return new NamedNode(localDeclarationStatement.GetName());
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
using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Collections;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LinqToWiki.Codegen
{
    /// <summary>
    /// Extension methods that help with creating of Roslyn syntax trees.
    /// </summary>
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
            return SyntaxFactory.List(nodes);
        }

        public static SeparatedSyntaxList<T> ToSeparatedList<T>(
            this IEnumerable<T> nodes, SyntaxKind separator = SyntaxKind.CommaToken)
            where T : SyntaxNode
        {
            var nodesArray = nodes == null ? new T[0] : nodes.ToArray();
            return SyntaxFactory.SeparatedList(
                nodesArray, Enumerable.Repeat(SyntaxFactory.Token(separator), Math.Max(nodesArray.Length - 1, 0)));
        }

        public static TNode WithDocumentationSummary<TNode>(this TNode node, string summary) where TNode : SyntaxNode
        {
            return node.WithLeadingTrivia(SyntaxFactory.Trivia(SyntaxEx.DocumentationComment(SyntaxEx.DocumentationSummary(summary))));
        }

        public static T SingleDescendant<T>(this SyntaxNode node) where T : SyntaxNode
        {
            return node.DescendantNodes().OfType<T>().Single();
        }
    }

    /// <summary>
    /// Contains methods for easy creating of Roslyn syntax trees.
    /// </summary>
    public static class SyntaxEx
    {
        public static FieldDeclarationSyntax FieldDeclaration(
            IEnumerable<SyntaxKind> modifiers, string typeName, string fieldName, ExpressionSyntax initializer = null)
        {
            return FieldDeclaration(modifiers, SyntaxFactory.ParseTypeName(typeName), fieldName, initializer);
        }

        public static FieldDeclarationSyntax FieldDeclaration(
            IEnumerable<SyntaxKind> modifiers, TypeSyntax type, string fieldName, ExpressionSyntax initializer = null)
        {
            return SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                    type, SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(fieldName))
                            .WithInitializer(
                                initializer == null ? null : SyntaxFactory.EqualsValueClause(initializer)))))
                .WithModifiers(TokenList(modifiers));
        }

        private static SyntaxTokenList TokenList(IEnumerable<SyntaxKind> modifiers)
        {
            return SyntaxFactory.TokenList(modifiers.Select(SyntaxFactory.Token));
        }

        private static SyntaxTokenList TokenList(SyntaxKind? modifier)
        {
            return modifier == null ? SyntaxFactory.TokenList() : SyntaxFactory.TokenList(SyntaxFactory.Token(modifier.Value));
        }

        public static ClassDeclarationSyntax ClassDeclaration(
            string className, TypeSyntax baseType, params MemberDeclarationSyntax[] members)
        {
            return ClassDeclaration(className, baseType, (IEnumerable<MemberDeclarationSyntax>)members);
        }

        public static ClassDeclarationSyntax ClassDeclaration(
            string className, params MemberDeclarationSyntax[] members)
        {
            return ClassDeclaration(className, (IEnumerable<MemberDeclarationSyntax>)members);
        }

        public static ClassDeclarationSyntax ClassDeclaration(
            SyntaxKind classType, string className, params MemberDeclarationSyntax[] members)
        {
            return ClassDeclaration(classType, className, (IEnumerable<MemberDeclarationSyntax>)members);
        }

        public static ClassDeclarationSyntax ClassDeclaration(
            SyntaxKind classType, string className, IEnumerable<MemberDeclarationSyntax> members)
        {
            return ClassDeclaration(classType, className, null, members);
        }

        public static ClassDeclarationSyntax ClassDeclaration(
            string className, IEnumerable<MemberDeclarationSyntax> members)
        {
            return ClassDeclaration(className, null, members);
        }

        private static ClassDeclarationSyntax ClassDeclaration(
            SyntaxKind classType, string className, TypeSyntax baseType, IEnumerable<MemberDeclarationSyntax> members)
        {
            return ClassDeclaration(classType, className, null, baseType, members);
        }

        public static ClassDeclarationSyntax ClassDeclaration(
            string className, TypeSyntax baseType, IEnumerable<MemberDeclarationSyntax> members)
        {
            return ClassDeclaration(className, null, baseType, members);
        }

        public static ClassDeclarationSyntax ClassDeclaration(
            string className, IEnumerable<TypeParameterSyntax> typeParameters, TypeSyntax baseType,
            IEnumerable<MemberDeclarationSyntax> members)
        {
            return ClassDeclaration(SyntaxKind.SealedKeyword, className, typeParameters, baseType, members);
        }

        public static ClassDeclarationSyntax ClassDeclaration(
            SyntaxKind classType, string className, IEnumerable<TypeParameterSyntax> typeParameters, TypeSyntax baseType,
            IEnumerable<MemberDeclarationSyntax> members)
        {
            var typeParameterListSyntax = typeParameters == null
                                              ? null
                                              : SyntaxFactory.TypeParameterList(typeParameters.ToSeparatedList());
            var baseTypeSyntax = baseType == null
                ? null
                : SyntaxFactory.BaseList(
                    SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(SyntaxFactory.SimpleBaseType(baseType)));

            var modifiers = new[] { SyntaxKind.PublicKeyword, classType };

            return SyntaxFactory.ClassDeclaration(className)
                .WithModifiers(TokenList(modifiers))
                .WithTypeParameterList(typeParameterListSyntax)
                .WithBaseList(baseTypeSyntax)
                .WithMembers(members.ToSyntaxList());
        }

        public static TypeParameterSyntax TypeParameter(string typeParameterName)
        {
            return SyntaxFactory.TypeParameter(SyntaxFactory.Identifier(typeParameterName));
        }

        public static EnumDeclarationSyntax EnumDeclaration(string enumName, IEnumerable<string> members)
        {
            return EnumDeclaration(
                enumName, members.Select(m => EnumMemberDeclaration(m)));
        }

        public static EnumDeclarationSyntax EnumDeclaration(
            string enumName, IEnumerable<EnumMemberDeclarationSyntax> members, SyntaxKind? baseType = null)
        {
            return SyntaxFactory.EnumDeclaration(enumName)
                .WithModifiers(TokenList(SyntaxKind.PublicKeyword))
                .WithBaseList(
                    baseType == null
                        ? null
                        : SyntaxFactory.BaseList(
                            SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                                SyntaxFactory.SimpleBaseType(
                                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(baseType.Value))))))
                .WithMembers(members.ToSeparatedList());
        }

        public static EnumMemberDeclarationSyntax EnumMemberDeclaration(
            string name, long? value = null, bool useHex = true)
        {
            var valueClause = value == null ? null : SyntaxFactory.EqualsValueClause(Literal(value.Value, useHex));
            return SyntaxFactory.EnumMemberDeclaration(name).WithEqualsValue(valueClause);
        }

        public static ConstructorDeclarationSyntax ConstructorDeclaration(
            IEnumerable<SyntaxKind> modifiers, string className, IEnumerable<ParameterSyntax> parameters = null,
            IEnumerable<StatementSyntax> statements = null, ConstructorInitializerSyntax constructorInitializer = null)
        {
            return SyntaxFactory.ConstructorDeclaration(className)
                .WithModifiers(TokenList(modifiers))
                .WithParameterList(SyntaxFactory.ParameterList(parameters.ToSeparatedList()))
                .WithBody(SyntaxFactory.Block(statements.ToSyntaxList()))
                .WithInitializer(constructorInitializer);
        }

        public static OperatorDeclarationSyntax OperatorDeclaration(
            TypeSyntax returnType, SyntaxKind operatorToken, IEnumerable<ParameterSyntax> parameters,
            IEnumerable<StatementSyntax> statements = null)
        {
            return SyntaxFactory.OperatorDeclaration(returnType, SyntaxFactory.Token(operatorToken))
                         .WithModifiers(TokenList(new[] { SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword }))
                         .WithParameterList(SyntaxFactory.ParameterList(parameters.ToSeparatedList()))
                         .WithBody(SyntaxFactory.Block(statements.ToSyntaxList()));
        }

        public static ConstructorInitializerSyntax ThisConstructorInitializer(params ExpressionSyntax[] arguments)
        {
            return ConstructorInitializer(SyntaxKind.ThisConstructorInitializer, arguments);
        }

        public static ConstructorInitializerSyntax BaseConstructorInitializer(params ExpressionSyntax[] arguments)
        {
            return ConstructorInitializer(SyntaxKind.BaseConstructorInitializer, arguments);
        }

        private static ConstructorInitializerSyntax ConstructorInitializer(
            SyntaxKind kind, IEnumerable<ExpressionSyntax> arguments)
        {
            return SyntaxFactory.ConstructorInitializer(
                kind, SyntaxFactory.ArgumentList(arguments.Select(SyntaxFactory.Argument).ToSeparatedList()));
        }

        public static ParameterSyntax Parameter(
            string typeName, string name, ExpressionSyntax defaultValue = null,
            IEnumerable<SyntaxKind> modifiers = null)
        {
            return Parameter(SyntaxFactory.ParseTypeName(typeName), name, defaultValue, modifiers);
        }

        public static ParameterSyntax Parameter(
            TypeSyntax type, string name, ExpressionSyntax defaultValue = null,
            IEnumerable<SyntaxKind> modifiers = null)
        {
            return SyntaxFactory.Parameter(SyntaxFactory.Identifier(name))
                .WithType(type)
                .WithDefault(defaultValue == null ? null : SyntaxFactory.EqualsValueClause(defaultValue))
                .WithModifiers(modifiers == null ? default(SyntaxTokenList) : TokenList(modifiers));
        }

        public static ExpressionStatementSyntax Assignment(NamedNode left, NamedNode right)
        {
            return Assignment(SyntaxFactory.IdentifierName(left.Name), SyntaxFactory.IdentifierName(right.Name));
        }

        public static ExpressionStatementSyntax Assignment(NamedNode left, ExpressionSyntax right)
        {
            return Assignment(SyntaxFactory.IdentifierName(left.Name), right);
        }

        public static ExpressionStatementSyntax Assignment(ExpressionSyntax left, ExpressionSyntax right)
        {
            return
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right));
        }

        public static ExpressionSyntax Coalesce(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SyntaxFactory.BinaryExpression(SyntaxKind.CoalesceExpression, left, right);
        }

        public static LiteralExpressionSyntax Literal(string value)
        {
            // TODO: escaping
            return SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal('"' + value + '"', value));
        }

        public static LiteralExpressionSyntax Literal(bool value)
        {
            return SyntaxFactory.LiteralExpression(
                value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
        }

        public static LiteralExpressionSyntax Literal(long value, bool useHex = false)
        {
            return SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(
                    string.Format(System.Globalization.CultureInfo.InvariantCulture, useHex ? "0x{0:X}" : "{0}", value),
                    value));
        }

        public static LiteralExpressionSyntax NullLiteral()
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
        }

        public static NamespaceDeclarationSyntax NamespaceDeclaration(
            string name, params MemberDeclarationSyntax[] members)
        {
            return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(name))
                .WithMembers(members.Where(m => m != null).ToSyntaxList());
        }

        public static CompilationUnitSyntax CompilationUnit(NamespaceDeclarationSyntax member, params string[] usings)
        {
            return SyntaxFactory.CompilationUnit()
                .WithUsings(usings.Select(u => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(u))).ToSyntaxList())
                .WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(member));
        }

        public static PropertyDeclarationSyntax AutoPropertyDeclaration(
            IEnumerable<SyntaxKind> modifiers, string typeName, string propertyName,
            SyntaxKind? setModifier = null, SyntaxKind? getModifier = null, bool isAbstract = false)
        {
            return AutoPropertyDeclaration(
                modifiers, SyntaxFactory.ParseTypeName(typeName), propertyName, setModifier, getModifier, isAbstract);
        }

        public static PropertyDeclarationSyntax AutoPropertyDeclaration(
            IEnumerable<SyntaxKind> modifiers, TypeSyntax type, string propertyName,
            SyntaxKind? setModifier = null, SyntaxKind? getModifier = null, bool isAbstract = false)
        {
            var accesors = new List<AccessorDeclarationSyntax>();
            if (!(isAbstract && getModifier == SyntaxKind.PrivateKeyword))
                accesors.Add(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithModifiers(TokenList(getModifier))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
            if (!(isAbstract && setModifier == SyntaxKind.PrivateKeyword))
                accesors.Add(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithModifiers(TokenList(setModifier))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

            return SyntaxFactory.PropertyDeclaration(type, propertyName)
                .WithModifiers(TokenList(modifiers))
                .WithAccessorList(SyntaxFactory.AccessorList(accesors.ToSyntaxList()));
        }

        public static PropertyDeclarationSyntax PropertyDeclaration(
            IEnumerable<SyntaxKind> modifiers, TypeSyntax type, string propertyName,
            IEnumerable<StatementSyntax> getStatements, SyntaxKind? getModifier = null,
            IEnumerable<StatementSyntax> setStatements = null, SyntaxKind? setModifier = null)
        {
            var accessors = new List<AccessorDeclarationSyntax>();

            accessors.Add(
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithModifiers(TokenList(getModifier))
                    .WithBody(SyntaxFactory.Block(getStatements.ToSyntaxList())));

            if (setStatements != null)
                accessors.Add(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithModifiers(TokenList(setModifier))
                    .WithBody(SyntaxFactory.Block(setStatements.ToSyntaxList())));

            return SyntaxFactory.PropertyDeclaration(type, propertyName)
                .WithModifiers(TokenList(modifiers))
                .WithAccessorList(SyntaxFactory.AccessorList(accessors.ToSyntaxList()));
        }

        public static MethodDeclarationSyntax MethodDeclaration(
            IEnumerable<SyntaxKind> modifiers, string returnTypeName, string methodName,
            IEnumerable<ParameterSyntax> parameters, params StatementSyntax[] statements)
        {
            return MethodDeclaration(
                modifiers, returnTypeName, methodName, parameters, (IEnumerable<StatementSyntax>)statements);
        }

        public static MethodDeclarationSyntax MethodDeclaration(
            IEnumerable<SyntaxKind> modifiers, string returnTypeName, string methodName,
            IEnumerable<ParameterSyntax> parameters, IEnumerable<StatementSyntax> statements)
        {
            return MethodDeclaration(
                modifiers, SyntaxFactory.ParseTypeName(returnTypeName), methodName, parameters, statements);
        }

        public static MethodDeclarationSyntax MethodDeclaration(
            IEnumerable<SyntaxKind> modifiers, TypeSyntax returnType, string methodName,
            IEnumerable<ParameterSyntax> parameters, params StatementSyntax[] statements)
        {
            return MethodDeclaration(modifiers, returnType, methodName, null, parameters, statements);
        }

        public static MethodDeclarationSyntax MethodDeclaration(
            IEnumerable<SyntaxKind> modifiers, TypeSyntax returnType, string methodName,
            IEnumerable<TypeParameterSyntax> typeParameters, IEnumerable<ParameterSyntax> parameters,
            params StatementSyntax[] statements)
        {
            return MethodDeclaration(
                modifiers, returnType, methodName, typeParameters, parameters, (IEnumerable<StatementSyntax>)statements);
        }

        public static MethodDeclarationSyntax MethodDeclaration(
            IEnumerable<SyntaxKind> modifiers, TypeSyntax returnType, string methodName,
            IEnumerable<ParameterSyntax> parameters, IEnumerable<StatementSyntax> statements)
        {
            return MethodDeclaration(modifiers, returnType, methodName, null, parameters, statements);
        }

        public static MethodDeclarationSyntax MethodDeclaration(
            IEnumerable<SyntaxKind> modifiers, TypeSyntax returnType, string methodName,
            IEnumerable<TypeParameterSyntax> typeParameters, IEnumerable<ParameterSyntax> parameters,
            IEnumerable<StatementSyntax> statements)
        {
            var typeParameterListSyntax =
                typeParameters == null
                    ? null
                    : SyntaxFactory.TypeParameterList(typeParameters.ToSeparatedList());
            var statmentsSyntax = statements == null ? null : SyntaxFactory.Block(statements.ToSyntaxList());
            var semicolonToken = statements == null ? SyntaxFactory.Token(SyntaxKind.SemicolonToken) : default(SyntaxToken);

            return SyntaxFactory.MethodDeclaration(returnType, methodName)
                .WithModifiers(TokenList(modifiers))
                .WithTypeParameterList(typeParameterListSyntax)
                .WithParameterList(SyntaxFactory.ParameterList(parameters.ToSeparatedList()))
                .WithBody(statmentsSyntax)
                .WithSemicolonToken(semicolonToken);
        }

        public static ObjectCreationExpressionSyntax ObjectCreation(
            string typeName, IEnumerable<ExpressionSyntax> arguments,
            IEnumerable<IEnumerable<ExpressionSyntax>> initializers = null)
        {
            return ObjectCreation(SyntaxFactory.ParseTypeName(typeName), arguments, initializers);
        }

        public static ObjectCreationExpressionSyntax ObjectCreation(
            string typeName, params ExpressionSyntax[] arguments)
        {
            return ObjectCreation(typeName, (IEnumerable<ExpressionSyntax>)arguments);
        }

        public static ObjectCreationExpressionSyntax ObjectCreation(
            TypeSyntax type, params ExpressionSyntax[] arguments)
        {
            return ObjectCreation(type, (IEnumerable<ExpressionSyntax>)arguments);
        }

        public static ObjectCreationExpressionSyntax ObjectCreation(
            TypeSyntax type, IEnumerable<ExpressionSyntax> arguments, IEnumerable<ExpressionSyntax> initializers)
        {
            return ObjectCreation(type, arguments, initializers.Select(i => new[] { i }));
        }

        public static ObjectCreationExpressionSyntax ObjectCreation(
            TypeSyntax type, IEnumerable<ExpressionSyntax> arguments,
            IEnumerable<IEnumerable<ExpressionSyntax>> initializers = null)
        {
            var argumentsArray = arguments == null ? new ExpressionSyntax[0] : arguments.ToArray();
            var argumentList =
                argumentsArray.Length == 0
                    ? SyntaxFactory.ArgumentList()
                    : SyntaxFactory.ArgumentList(argumentsArray.Select(SyntaxFactory.Argument).ToSeparatedList());

            InitializerExpressionSyntax initializer =
                initializers == null
                    ? null
                    : SyntaxFactory.InitializerExpression(
                        SyntaxKind.CollectionInitializerExpression, initializers.Select(ToInitializer).ToSeparatedList());

            return SyntaxFactory.ObjectCreationExpression(type, argumentList, initializer);
        }

        private static ExpressionSyntax ToInitializer(IEnumerable<ExpressionSyntax> expressions)
        {
            var expressionsArray = expressions.ToArray();

            if (expressionsArray.Length == 1)
                return expressionsArray[0];

            return SyntaxFactory.InitializerExpression(
                SyntaxKind.ObjectInitializerExpression, expressionsArray.ToSeparatedList());
        }

        public static ExpressionSyntax ArrayCreation(string typeName, IEnumerable<ExpressionSyntax> expressions)
        {
            var initializer = SyntaxFactory.InitializerExpression(
                SyntaxKind.ArrayInitializerExpression, expressions.ToSeparatedList());

            if (string.IsNullOrEmpty(typeName))
                return SyntaxFactory.ImplicitArrayCreationExpression(initializer);

            return SyntaxFactory.ArrayCreationExpression(
                SyntaxFactory.ArrayType(SyntaxFactory.ParseTypeName(typeName), SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier())),
                initializer);
        }

        public static LocalDeclarationStatementSyntax LocalDeclaration(
            string type, string localName, ExpressionSyntax value)
        {
            return SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.ParseTypeName(type),
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(localName))
                            .WithInitializer(SyntaxFactory.EqualsValueClause(value)))));
        }

        public static ReturnStatementSyntax Return(NamedNode namedNode)
        {
            return SyntaxFactory.ReturnStatement(namedNode);
        }

        public static ReturnStatementSyntax Return(ExpressionSyntax expression)
        {
            return SyntaxFactory.ReturnStatement(expression);
        }

        public static ThrowStatementSyntax Throw(ExpressionSyntax expression)
        {
            return SyntaxFactory.ThrowStatement(expression);
        }

        public static IfStatementSyntax If(
            ExpressionSyntax condition, StatementSyntax ifStatement, StatementSyntax elseStatement = null)
        {
            return SyntaxFactory.IfStatement(
                condition, ifStatement, elseStatement == null ? null : SyntaxFactory.ElseClause(elseStatement));
        }

        public static SwitchStatementSyntax Switch(
            ExpressionSyntax expression, IEnumerable<SwitchSectionSyntax> sections)
        {
            return SyntaxFactory.SwitchStatement(expression, sections.ToSyntaxList());
        }

        public static SwitchSectionSyntax SwitchCase(ExpressionSyntax value, params StatementSyntax[] statements)
        {
            return SyntaxFactory.SwitchSection(
                SyntaxFactory.SingletonList<SwitchLabelSyntax>(SyntaxFactory.CaseSwitchLabel(value)),
                statements.ToSyntaxList());
        }

        public static BlockSyntax Block(IEnumerable<StatementSyntax> statements)
        {
            return SyntaxFactory.Block(statements.ToSyntaxList());
        }

        public static InvocationExpressionSyntax Invocation(
            ExpressionSyntax expression, params ExpressionSyntax[] arguments)
        {
            return Invocation(expression, (IEnumerable<ExpressionSyntax>)arguments);
        }

        public static InvocationExpressionSyntax Invocation(
            ExpressionSyntax expression, IEnumerable<ExpressionSyntax> arguments)
        {
            return SyntaxFactory.InvocationExpression(
                expression,
                SyntaxFactory.ArgumentList(arguments.Select(SyntaxFactory.Argument).ToSeparatedList()));
        }

        public static ElementAccessExpressionSyntax ElementAccess(
            ExpressionSyntax expression, params ExpressionSyntax[] arguments)
        {
            return SyntaxFactory.ElementAccessExpression(
                expression,
                SyntaxFactory.BracketedArgumentList(arguments.Select(SyntaxFactory.Argument).ToSeparatedList()));
        }

        public static MemberAccessExpressionSyntax MemberAccess(string name, string memberName)
        {
            return MemberAccess(SyntaxFactory.ParseExpression(name), memberName);
        }

        public static MemberAccessExpressionSyntax MemberAccess(NamedNode namedNode, string memberName)
        {
            return MemberAccess((ExpressionSyntax)namedNode, memberName);
        }

        public static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax expression, string memberName)
        {
            return MemberAccess(expression, (SimpleNameSyntax)SyntaxFactory.ParseName(memberName));
        }

        public static MemberAccessExpressionSyntax MemberAccess(string name, SimpleNameSyntax memberName)
        {
            return MemberAccess(SyntaxFactory.ParseExpression(name), memberName);
        }

        public static MemberAccessExpressionSyntax MemberAccess(
            ExpressionSyntax expression, SimpleNameSyntax memberName)
        {
            return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, memberName);
        }

        public static BinaryExpressionSyntax Equals(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, left, right);
        }

        public static BinaryExpressionSyntax NotEquals(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, left, right);
        }

        public static BinaryExpressionSyntax And(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, left, right);
        }

        public static PrefixUnaryExpressionSyntax Not(ExpressionSyntax operand)
        {
            return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, operand);
        }

        public static TypeOfExpressionSyntax TypeOf(string typeName)
        {
            return SyntaxFactory.TypeOfExpression(SyntaxFactory.ParseTypeName(typeName));
        }

        public static CastExpressionSyntax Cast(string typeName, ExpressionSyntax expression)
        {
            return SyntaxFactory.CastExpression(SyntaxFactory.ParseTypeName(typeName), expression);
        }

        public static GenericNameSyntax GenericName(string name, params string[] typeArgumentNames)
        {
            return GenericName(name, (IEnumerable<string>)typeArgumentNames);
        }

        public static GenericNameSyntax GenericName(string name, IEnumerable<string> typeArgumentNames)
        {
            return GenericName(name, typeArgumentNames.Select(SyntaxFactory.IdentifierName));
        }

        public static GenericNameSyntax GenericName(string name, params TypeSyntax[] typeArguments)
        {
            return GenericName(name, (IEnumerable<TypeSyntax>)typeArguments);
        }

        public static GenericNameSyntax GenericName(string name, IEnumerable<TypeSyntax> typeArguments)
        {
            return SyntaxFactory.GenericName(
                SyntaxFactory.Identifier(name), SyntaxFactory.TypeArgumentList(typeArguments.ToSeparatedList()));
        }

        public static XmlElementSyntax DocumentationSummary(string summary)
        {
            return DocumentationElement("summary", summary);
        }

        public static XmlElementSyntax DocumentationParameter(string name, string text)
        {
            return DocumentationElement("param ", text, name);
        }

        private static SyntaxToken XmlTextNewLine()
        {
            return SyntaxFactory.XmlTextNewLine(
                SyntaxFactory.TriviaList(), Environment.NewLine, Environment.NewLine, SyntaxFactory.TriviaList());
        }

        public static DocumentationCommentTriviaSyntax DocumentationComment(params XmlElementSyntax[] elements)
        {
            return DocumentationComment((IEnumerable<XmlElementSyntax>)elements);
        }

        public static DocumentationCommentTriviaSyntax DocumentationComment(IEnumerable<XmlElementSyntax> elements)
        {
            return SyntaxFactory.DocumentationCommentTrivia(
                SyntaxKind.SingleLineDocumentationCommentTrivia,
                elements.AddAfterEach<XmlNodeSyntax>(SyntaxFactory.XmlText(SyntaxFactory.TokenList(XmlTextNewLine())))
                    .ToSyntaxList());
        }

        private static XmlElementSyntax DocumentationElement(
            string elementName, string text, string name = null)
        {
            var nameSyntax = XmlName(elementName);
            var exteriorTrivia = SyntaxFactory.DocumentationCommentExterior("///");

            string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var tokens = new List<SyntaxToken>();

            tokens.Add(XmlTextNewLine());

            foreach (var line in lines)
            {
                var lineToken = XmlText(line)
                    .WithLeadingTrivia(exteriorTrivia);
                tokens.Add(lineToken);

                tokens.Add(XmlTextNewLine());
            }

            var attributeSyntaxes = new List<XmlAttributeSyntax>();

            if (name != null)
                attributeSyntaxes.Add(
                    SyntaxFactory.XmlNameAttribute(
                        XmlName("name"), SyntaxFactory.Token(SyntaxKind.DoubleQuoteToken),
                        name, SyntaxFactory.Token(SyntaxKind.DoubleQuoteToken)));

            return SyntaxFactory.XmlElement(
                SyntaxFactory.XmlElementStartTag(nameSyntax, attributeSyntaxes.ToSyntaxList())
                    .WithLeadingTrivia(exteriorTrivia),
                SyntaxFactory.SingletonList<XmlNodeSyntax>(SyntaxFactory.XmlText(SyntaxFactory.TokenList(tokens))),
                SyntaxFactory.XmlElementEndTag(nameSyntax).WithLeadingTrivia(exteriorTrivia));
        }

        private static SyntaxToken XmlText(string text)
        {
            return SyntaxFactory.XmlTextLiteral(new SyntaxTriviaList(), text, text, new SyntaxTriviaList());
        }

        private static XmlNameSyntax XmlName(string name)
        {
            return SyntaxFactory.XmlName(name);
        }

        public static SimpleLambdaExpressionSyntax LambdaExpression(string parameterName, CSharpSyntaxNode body)
        {
            return SyntaxFactory.SimpleLambdaExpression(
                SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName)), body);
        }
    }

    /// <summary>
    /// Class that represents reference to a named symbol, like a local variable or a field.
    /// Can be used for referencing such node after its creation.
    /// 
    /// Is implicitly convertible from types declaring those symbols and to <see cref="IdentifierNameSyntax"/>,
    /// that's be used to reference them.
    /// </summary>
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
            return SyntaxFactory.IdentifierName(namedNode.Name);
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
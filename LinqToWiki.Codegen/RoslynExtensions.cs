using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Collections;
using Roslyn.Compilers.CSharp;

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

        public static EnumDeclarationSyntax WithAttribute(this EnumDeclarationSyntax enumDeclaration, AttributeDeclarationSyntax attribute)
        {
            return enumDeclaration.Update(
                enumDeclaration.Attributes.Concat(new[] { attribute }).ToSyntaxList(), enumDeclaration.Modifiers,
                enumDeclaration.EnumKeyword, enumDeclaration.Identifier, enumDeclaration.BaseListOpt,
                enumDeclaration.OpenBraceToken, enumDeclaration.Members, enumDeclaration.CloseBraceToken,
                enumDeclaration.SemicolonTokenOpt);
        }

        public static TNode WithDocumentationSummary<TNode>(this TNode node, string summary) where TNode : SyntaxNode
        {
            return node.WithLeadingTrivia(Syntax.Trivia(SyntaxEx.DocumentationComment(SyntaxEx.DocumentationSummary(summary))));
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
            return Syntax.TokenList(modifiers.Select(Syntax.Token));
        }

        private static SyntaxTokenList TokenList(SyntaxKind? modifier)
        {
            return modifier == null ? Syntax.TokenList() : Syntax.TokenList(Syntax.Token(modifier.Value));
        }

        public static ClassDeclarationSyntax ClassDeclaration(string className, TypeSyntax baseType, params MemberDeclarationSyntax[] members)
        {
            return ClassDeclaration(className, baseType, (IEnumerable<MemberDeclarationSyntax>)members);
        }

        public static ClassDeclarationSyntax ClassDeclaration(string className, params MemberDeclarationSyntax[] members)
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

        public static ClassDeclarationSyntax ClassDeclaration(string className, IEnumerable<MemberDeclarationSyntax> members)
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
                                              : Syntax.TypeParameterList(parameters: typeParameters.ToSeparatedList());
            var baseTypeSyntax = baseType == null ? null : Syntax.BaseList(types: new[] { baseType }.ToSeparatedList());

            var modifiers = new[] { SyntaxKind.PublicKeyword, classType };

            return Syntax.ClassDeclaration(
                modifiers: TokenList(modifiers),
                identifier: Syntax.Identifier(className),
                typeParameterListOpt: typeParameterListSyntax,
                baseListOpt: baseTypeSyntax,
                members: members.ToSyntaxList());
        }

        public static TypeParameterSyntax TypeParameter(string typeParameterName)
        {
            return Syntax.TypeParameter(identifier: Syntax.Identifier(typeParameterName));
        }

        public static EnumDeclarationSyntax EnumDeclaration(string enumName, IEnumerable<string> members)
        {
            return EnumDeclaration(
                enumName, members.Select(m => EnumMemberDeclaration(m)));
        }

        public static EnumDeclarationSyntax EnumDeclaration(
            string enumName, IEnumerable<EnumMemberDeclarationSyntax> members, SyntaxKind? baseType = null)
        {
            return Syntax.EnumDeclaration(
                modifiers: TokenList(SyntaxKind.PublicKeyword),
                identifier: Syntax.Identifier(enumName),
                baseListOpt:
                    baseType == null
                        ? null
                        : Syntax.BaseList(
                            types:
                                new TypeSyntax[] { Syntax.PredefinedType(Syntax.Token(baseType.Value)) }
                                .ToSeparatedList()),
                members: members.ToSeparatedList());
        }

        public static EnumMemberDeclarationSyntax EnumMemberDeclaration(string name, long? value = null, bool useHex = true)
        {
            var valueClause = value == null ? null : Syntax.EqualsValueClause(value: Literal(value.Value, useHex));
            return Syntax.EnumMemberDeclaration(identifier: Syntax.Identifier(name), equalsValueOpt: valueClause);
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
            return ConstructorInitializer(SyntaxKind.ThisConstructorInitializer, arguments);
        }

        public static ConstructorInitializerSyntax BaseConstructorInitializer(params ExpressionSyntax[] arguments)
        {
            return ConstructorInitializer(SyntaxKind.BaseConstructorInitializer, arguments);
        }

        private static ConstructorInitializerSyntax ConstructorInitializer(SyntaxKind kind, IEnumerable<ExpressionSyntax> arguments)
        {
            return Syntax.ConstructorInitializer(
                kind,
                argumentList: Syntax.ArgumentList(
                    arguments: arguments.Select(a => Syntax.Argument(expression: a)).ToSeparatedList()));
        }

        public static ParameterSyntax Parameter(
            string typeName, string name, ExpressionSyntax defaultValue = null,
            IEnumerable<SyntaxKind> modifiers = null)
        {
            return Parameter(Syntax.ParseTypeName(typeName), name, defaultValue, modifiers);
        }

        public static ParameterSyntax Parameter(
            TypeSyntax type, string name, ExpressionSyntax defaultValue = null,
            IEnumerable<SyntaxKind> modifiers = null)
        {
            return Syntax.Parameter(
                typeOpt: type,
                identifier: Syntax.Identifier(name),
                defaultOpt: defaultValue == null ? null : Syntax.EqualsValueClause(value: defaultValue),
                modifiers:
                    modifiers == null ? default(SyntaxTokenList) : TokenList(modifiers));
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

        public static ExpressionSyntax Coalesce(ExpressionSyntax left, ExpressionSyntax right)
        {
            return Syntax.BinaryExpression(SyntaxKind.CoalesceExpression, left, right: right);
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

        public static LiteralExpressionSyntax Literal(long value, bool useHex = false)
        {
            return Syntax.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                Syntax.Literal(
                    string.Format(System.Globalization.CultureInfo.InvariantCulture, useHex ? "0x{0:X}" : "{0}", value),
                    value));
        }

        public static LiteralExpressionSyntax NullLiteral()
        {
            return Syntax.LiteralExpression(SyntaxKind.NullLiteralExpression);
        }

        public static NamespaceDeclarationSyntax NamespaceDeclaration(string name, params MemberDeclarationSyntax[] members)
        {
            return Syntax.NamespaceDeclaration(
                name: Syntax.ParseName(name), members: members.Where(m => m != null).ToSyntaxList());
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
            return AutoPropertyDeclaration(
                modifiers, Syntax.ParseTypeName(typeName), propertyName, setModifier, getModifier);
        }

        public static PropertyDeclarationSyntax AutoPropertyDeclaration(
            IEnumerable<SyntaxKind> modifiers, TypeSyntax type, string propertyName,
            SyntaxKind? setModifier = null, SyntaxKind? getModifier = null, bool isAbstarct = false)
        {
            var accesors = new List<AccessorDeclarationSyntax>();
            if (!(isAbstarct && getModifier == SyntaxKind.PrivateKeyword))
                accesors.Add(
                    Syntax.AccessorDeclaration(
                        SyntaxKind.GetAccessorDeclaration,
                        modifiers: TokenList(getModifier),
                        semicolonTokenOpt: Syntax.Token(SyntaxKind.SemicolonToken)));
            if (!(isAbstarct && setModifier == SyntaxKind.PrivateKeyword))
                accesors.Add(
                    Syntax.AccessorDeclaration(
                        SyntaxKind.SetAccessorDeclaration,
                        modifiers: TokenList(setModifier),
                        semicolonTokenOpt: Syntax.Token(SyntaxKind.SemicolonToken)));

            return Syntax.PropertyDeclaration(
                modifiers: TokenList(modifiers), type: type,
                identifier: Syntax.Identifier(propertyName),
                accessorList: Syntax.AccessorList(accessors: accesors.ToSyntaxList()));
        }

        public static PropertyDeclarationSyntax PropertyDeclaration(
            IEnumerable<SyntaxKind> modifiers, TypeSyntax type, string propertyName,
            IEnumerable<StatementSyntax> getStatements, SyntaxKind? getModifier = null,
            IEnumerable<StatementSyntax> setStatements = null, SyntaxKind? setModifier = null)
        {
            var accessors = new List<AccessorDeclarationSyntax>();

            accessors.Add(
                Syntax.AccessorDeclaration(
                    SyntaxKind.GetAccessorDeclaration, modifiers: TokenList(getModifier),
                    bodyOpt: Syntax.Block(statements: getStatements.ToSyntaxList())));

            if (setStatements != null)
                accessors.Add(
                    Syntax.AccessorDeclaration(
                        SyntaxKind.SetAccessorDeclaration, modifiers: TokenList(setModifier),
                        bodyOpt: Syntax.Block(statements: setStatements.ToSyntaxList())));

            return Syntax.PropertyDeclaration(
                modifiers: TokenList(modifiers), type: type,
                identifier: Syntax.Identifier(propertyName),
                accessorList: Syntax.AccessorList(accessors: accessors.ToSyntaxList()));
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
                modifiers, Syntax.ParseTypeName(returnTypeName), methodName, parameters, statements);
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
            var typeParameterListSyntax = typeParameters == null
                                              ? null
                                              : Syntax.TypeParameterList(parameters: typeParameters.ToSeparatedList());
            var statmentsSyntax = statements == null ? null : Syntax.Block(statements: statements.ToSyntaxList());
            var semicolonToken = statements == null ? Syntax.Token(SyntaxKind.SemicolonToken) : default(SyntaxToken);

            return Syntax.MethodDeclaration(
                modifiers: TokenList(modifiers),
                returnType: returnType,
                identifier: Syntax.Identifier(methodName),
                typeParameterListOpt: typeParameterListSyntax,
                parameterList: Syntax.ParameterList(parameters: parameters.ToSeparatedList()),
                bodyOpt: statmentsSyntax,
                semicolonTokenOpt:semicolonToken);
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
                    ParseTypeName(type),
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

        public static ThrowStatementSyntax Throw(ExpressionSyntax expression)
        {
            return Syntax.ThrowStatement(expressionOpt: expression);
        }

        public static IfStatementSyntax If(
            ExpressionSyntax condition, StatementSyntax ifStatement, StatementSyntax elseStatement = null)
        {
            return Syntax.IfStatement(
                condition: condition,
                statement: ifStatement,
                elseOpt: elseStatement == null ? null : Syntax.ElseClause(statement: elseStatement));
        }

        public static SwitchStatementSyntax Switch(ExpressionSyntax expression, IEnumerable<SwitchSectionSyntax> sections)
        {
            return Syntax.SwitchStatement(expression: expression, sections: sections.ToSyntaxList());
        }

        public static SwitchSectionSyntax SwitchCase(ExpressionSyntax value, params StatementSyntax[] statements)
        {
            return Syntax.SwitchSection(
                new[] { Syntax.SwitchLabel(SyntaxKind.CaseSwitchLabel, valueOpt: value) }.ToSyntaxList(),
                statements: statements.ToSyntaxList());
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

        public static BinaryExpressionSyntax And(ExpressionSyntax left, ExpressionSyntax right)
        {
            return Syntax.BinaryExpression(SyntaxKind.LogicalAndExpression, left: left, right: right);
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
            return GenericName(name, (IEnumerable<string>)typeArgumentNames);
        }

        public static GenericNameSyntax GenericName(string name, IEnumerable<string> typeArgumentNames)
        {
            return GenericName(name, typeArgumentNames.Select(Syntax.IdentifierName));
        }

        public static GenericNameSyntax GenericName(string name, params TypeSyntax[] typeArguments)
        {
            return GenericName(name, (IEnumerable<TypeSyntax>)typeArguments);
        }

        public static GenericNameSyntax GenericName(string name, IEnumerable<TypeSyntax> typeArguments)
        {
            return Syntax.GenericName(
                Syntax.Identifier(name), Syntax.TypeArgumentList(arguments: typeArguments.ToSeparatedList()));
        }

        public static XmlElementSyntax DocumentationSummary(string summary)
        {
            return DocumentationElement("summary", summary);
        }

        public static XmlElementSyntax DocumentationParameter(string name, string text)
        {
            return DocumentationElement("param", text, new TupleList<string, string> { { "name", name } });
        }

        private static SyntaxToken XmlTextNewLine()
        {
            return Syntax.XmlTextNewLine(
                Syntax.TriviaList(), Environment.NewLine, Environment.NewLine, Syntax.TriviaList());
        }

        public static DocumentationCommentSyntax DocumentationComment(params XmlElementSyntax[] elements)
        {
            return DocumentationComment((IEnumerable<XmlElementSyntax>)elements);
        }

        public static DocumentationCommentSyntax DocumentationComment(IEnumerable<XmlElementSyntax> elements)
        {
            return Syntax.DocumentationComment(
                elements.Concat(new XmlNodeSyntax[] { Syntax.XmlText(Syntax.TokenList(XmlTextNewLine())) })
                    .ToSyntaxList());
        }

        private static XmlElementSyntax DocumentationElement(string elementName, string text, IEnumerable<Tuple<string, string>> attributes = null)
        {
            var nameSyntax = XmlName(elementName);
            var exteriorTrivia = Syntax.DocumentationCommentExteriorTrivia("///");

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

            if (attributes != null)
                foreach (var attribute in attributes)
                    attributeSyntaxes.Add(
                        Syntax.XmlAttribute(
                            XmlName(attribute.Item1),
                            startQuoteToken: Syntax.Token(SyntaxKind.DoubleQuoteToken),
                            textTokens: Syntax.TokenList(XmlText(attribute.Item2)),
                            endQuoteToken: Syntax.Token(SyntaxKind.DoubleQuoteToken)));

            return Syntax.XmlElement(
                Syntax.XmlElementStartTag(name: nameSyntax, attributes: attributeSyntaxes.ToSyntaxList())
                    .WithLeadingTrivia(exteriorTrivia),
                new XmlNodeSyntax[]
                {
                    Syntax.XmlText(Syntax.TokenList(tokens))
                }.ToSyntaxList(),
                Syntax.XmlElementEndTag(name: nameSyntax).WithLeadingTrivia(exteriorTrivia));
        }

        private static SyntaxToken XmlText(string text)
        {
            return Syntax.XmlText(new SyntaxTriviaList(), text, text, new SyntaxTriviaList());
        }

        private static XmlNameSyntax XmlName(string name)
        {
            return Syntax.XmlName(localName: Syntax.Identifier(name));
        }

        public static AttributeDeclarationSyntax AttributeDeclaration(string name, params AttributeArgumentSyntax[] arguments)
        {
            return Syntax.AttributeDeclaration(
                attributes:
                    new[]
                    {
                        Syntax.Attribute(
                            Syntax.IdentifierName(name),
                            arguments.Length == 0
                                ? null
                                : Syntax.AttributeArgumentList(arguments: arguments.ToSeparatedList()))
                    }.ToSeparatedList());
        }

        public static AttributeArgumentSyntax AttributeArgument(ExpressionSyntax expression, string name = null)
        {
            var nameSyntax = name == null ? null : Syntax.NameEquals(Syntax.Identifier(name));
            return Syntax.AttributeArgument(expression: expression, nameEqualsOpt: nameSyntax);
        }

        public static SimpleLambdaExpressionSyntax LambdaExpression(string parameterName, SyntaxNode body)
        {
            return Syntax.SimpleLambdaExpression(
                Syntax.Parameter(identifier: Syntax.Identifier(parameterName)), body: body);
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
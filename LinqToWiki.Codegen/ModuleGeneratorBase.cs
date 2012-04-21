using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Codegen.ModuleInfo;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen
{
    public abstract class ModuleGeneratorBase
    {
        protected Wiki Wiki { get; private set; }
        protected string ClassNameBase { get; private set; }

        protected ModuleGeneratorBase(Wiki wiki)
        {
            Wiki = wiki;
        }

        public void Generate(Module module)
        {
            if (module.PropertyGroups == null)
                return;

            ClassNameBase = module.Name;

            GenerateInternal(module);

            Wiki.ModuleFinished();
        }

        protected abstract void GenerateInternal(Module module);

        protected ClassDeclarationSyntax GenerateClassForProperties(string className, IEnumerable<Property> properties)
        {
            var propertiesArray = properties.ToArray();

            return SyntaxEx.ClassDeclaration(
                className, propertiesArray.Select(p => GenerateProperty(p.Name, p.Type, p.Nullable)))
                .WithPrivateConstructor()
                .WithAdditionalMembers(GenerateParseMethod(className, propertiesArray), GenerateToStringMethod(propertiesArray));
        }

        private MethodDeclarationSyntax GenerateParseMethod(string className, IEnumerable<Property> properties)
        {
            var elementParameter = SyntaxEx.Parameter("XElement", "element");
            var wikiParameter = SyntaxEx.Parameter("WikiInfo", "wiki");
            var statements = new List<StatementSyntax>();

            var resultLocal = SyntaxEx.LocalDeclaration("var", "result", SyntaxEx.ObjectCreation(className));

            statements.Add(resultLocal);

            foreach (var property in properties)
            {
                ExpressionSyntax propertyValueAccess;
                bool checkForNull;

                if (property.Name == "*")
                {
                    propertyValueAccess = (NamedNode)elementParameter;
                    checkForNull = false;
                }
                else
                {
                    propertyValueAccess = SyntaxEx.Invocation(
                        SyntaxEx.MemberAccess(elementParameter, "Attribute"), SyntaxEx.Literal(property.Name));
                    checkForNull = true;
                }
                var propertyValueLocal = SyntaxEx.LocalDeclaration("var", GetPropertyName(property.Name) + "Value", propertyValueAccess);

                statements.Add(propertyValueLocal);

                var assignment = SyntaxEx.Assignment(
                    SyntaxEx.MemberAccess(resultLocal, GetPropertyName(property.Name)),
                    Wiki.TypeManager.CreateConverter(
                        property, SyntaxEx.MemberAccess(propertyValueLocal, "Value"), (NamedNode)wikiParameter));

                if (checkForNull)
                {
                    var ifStatement = SyntaxEx.If(
                        SyntaxEx.NotEquals((NamedNode)propertyValueLocal, SyntaxEx.NullLiteral()), assignment);
                    statements.Add(ifStatement);
                }
                else
                    statements.Add(assignment);
            }

            statements.Add(SyntaxEx.Return(resultLocal));

            return SyntaxEx.MethodDeclaration(
                new[] { SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword }, className, "Parse",
                new[] { elementParameter, wikiParameter }, statements);
        }

        private static MethodDeclarationSyntax GenerateToStringMethod(IEnumerable<Property> properties)
        {
            var formatString = string.Join(
                "; ", properties.Select((p, i) => string.Format("{0}: {{{1}}}", GetPropertyName(p.Name), i)));

            var parameters = new List<ExpressionSyntax> { SyntaxEx.Literal(formatString) };

            parameters.AddRange(properties.Select(p => Syntax.IdentifierName(GetPropertyName(p.Name))));

            var returnStatement = SyntaxEx.Return(SyntaxEx.Invocation(SyntaxEx.MemberAccess("string", "Format"), parameters));

            return SyntaxEx.MethodDeclaration(
                new[] { SyntaxKind.PublicKeyword, SyntaxKind.OverrideKeyword }, "string", "ToString", null,
                returnStatement);
        }

        protected PropertyDeclarationSyntax GenerateProperty(string name, ParameterType type, bool nullable = false, string description = null)
        {
            var result = SyntaxEx.AutoPropertyDeclaration(
                new[] { SyntaxKind.PublicKeyword }, Wiki.TypeManager.GetTypeName(type, name, nullable),
                GetPropertyName(name), SyntaxKind.PrivateKeyword);

            if (description != null)
                result = result.WithDocumentationSummary(description);

            return result;
        }

        private static string GetPropertyName(string name)
        {
            if (name == "*")
                return "value";
            if (name == "namespace")
                return "ns";
            if (name == "default")
                return "defaultvalue";

            return name;
        }

        protected void GenerateMethod(
            Module module, IEnumerable<Parameter> methodParameters, string resultClassName,
            FieldDeclarationSyntax propsField, string fileName, bool nullableParameters)
        {
            var queryActionFile = Wiki.Files[fileName];
            var queryActionClass = queryActionFile.SingleDescendant<ClassDeclarationSyntax>();

            var queryTypePropertiesType = SyntaxEx.GenericName("QueryTypeProperties", resultClassName);

            var propertiesInitializer = SyntaxEx.ObjectCreation(
                queryTypePropertiesType,
                SyntaxEx.Literal(module.Name),
                SyntaxEx.Literal(module.Prefix),
                module.QueryType == null
                    ? (ExpressionSyntax)SyntaxEx.NullLiteral()
                    : SyntaxEx.MemberAccess("QueryType", module.QueryType.ToString()),
                CreateTupleListExpression(GetBaseParameters(module)),
                propsField == null ? (ExpressionSyntax)SyntaxEx.NullLiteral() : (NamedNode)propsField,
                SyntaxEx.MemberAccess(resultClassName, "Parse"));

            var propertiesField =
                SyntaxEx.FieldDeclaration(
                    new[] { SyntaxKind.PrivateKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword },
                    queryTypePropertiesType, ClassNameBase + "Properties", propertiesInitializer);

            ExpressionSyntax queryParameters = SyntaxEx.Invocation(
                SyntaxEx.MemberAccess("QueryParameters", SyntaxEx.GenericName("Create", resultClassName)));

            var queryParametersLocal = SyntaxEx.LocalDeclaration("var", "queryParameters", queryParameters);

            var documentationElements = new List<XmlElementSyntax>();

            var summary = SyntaxEx.DocumentationSummary(module.Description);
            documentationElements.Add(summary);

            var parameters = new List<ParameterSyntax>();
            var statements = new List<StatementSyntax>();

            statements.Add(queryParametersLocal);

            foreach (var methodParameter in methodParameters)
            {
                var parameter = SyntaxEx.Parameter(
                    Wiki.TypeManager.GetTypeName(methodParameter), methodParameter.Name,
                    nullableParameters ? SyntaxEx.NullLiteral() : null);

                parameters.Add(parameter);

                var queryParametersAssignment = SyntaxEx.Assignment(
                    queryParametersLocal, SyntaxEx.Invocation(
                        SyntaxEx.MemberAccess(queryParametersLocal, "AddSingleValue"),
                        SyntaxEx.Literal(methodParameter.Name),
                        SyntaxEx.Invocation(SyntaxEx.MemberAccess(parameter, "ToQueryString"))));

                if (nullableParameters)
                {
                    var assignmentWithCheck = SyntaxEx.If(
                        SyntaxEx.NotEquals((NamedNode)parameter, SyntaxEx.NullLiteral()), queryParametersAssignment);

                    statements.Add(assignmentWithCheck);
                }
                else
                    statements.Add(queryParametersAssignment);

                var parameterDocumentation = SyntaxEx.DocumentationParameter(
                    methodParameter.Name, methodParameter.Description);

                documentationElements.Add(parameterDocumentation);
            }

            var queryCreation = GenerateMethodResult(
                SyntaxEx.ObjectCreation(
                    SyntaxEx.GenericName("QueryProcessor", resultClassName),
                    Syntax.IdentifierName("m_wiki"),
                    (NamedNode)propertiesField), (NamedNode)queryParametersLocal);

            statements.Add(SyntaxEx.Return(queryCreation));

            var method = SyntaxEx.MethodDeclaration(
                new[] { SyntaxKind.PublicKeyword }, GenerateMethodResultType(), ClassNameBase, parameters, statements)
                .WithLeadingTrivia(Syntax.Trivia(SyntaxEx.DocumentationComment(documentationElements)));

            Wiki.Files[fileName] = queryActionFile.ReplaceNode(
                queryActionClass, queryActionClass.WithAdditionalMembers(propertiesField, method));
        }

        protected abstract IEnumerable<Tuple<string, string>> GetBaseParameters(Module module);

        protected abstract ExpressionSyntax GenerateMethodResult(ExpressionSyntax queryProcessor, ExpressionSyntax queryParameters);

        protected abstract TypeSyntax GenerateMethodResultType();

        private static ObjectCreationExpressionSyntax CreateTupleListExpression(IEnumerable<Tuple<string, string>> tupleList)
        {
            return SyntaxEx.ObjectCreation(
                "TupleList<string, string>", null,
                tupleList.Select(t => new[] { t.Item1, t.Item2 }.Select(SyntaxEx.Literal)));
        }
    }

    internal static class ModuleGeneratorRoslynExtensions
    {
        public static ClassDeclarationSyntax WithPrivateConstructor(this ClassDeclarationSyntax selectClass)
        {
            return selectClass.WithAdditionalMembers(
                SyntaxEx.ConstructorDeclaration(new[] { SyntaxKind.PrivateKeyword }, selectClass.Identifier.ValueText));
        }
    }
}
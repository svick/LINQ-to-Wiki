using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Collections;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen
{
    class ListModuleGenerator
    {
        private readonly Wiki m_wiki;

        private string m_typeNameBase;
        private string m_selectClassName;
        private string m_whereClassName;
        private string m_orderByClassName;

        private FieldDeclarationSyntax m_selectProps;

        public ListModuleGenerator(Wiki wiki)
        {
            m_wiki = wiki;
        }

        public void Generate(Module module)
        {
            if (module.PropertyGroups == null)
                return;

            m_typeNameBase = GetTypeNameBase(module);
            m_selectClassName = m_typeNameBase + "Select";
            m_whereClassName = m_typeNameBase + "Where";
            m_orderByClassName = m_typeNameBase + "OrderBy";

            var parameters = module.Parameters.ToList();

            var sortParameters = RemoveAndReturnByNames(parameters, "sort", "dir");

            var methodParameters = parameters.RemoveAndReturn(p => p.Required);

            // don't belong anywhere, are used in a special way
            RemoveAndReturnByNames(parameters, "continue", "limit", "prop");

            var whereParameters = parameters;

            var selectClass = GenerateSelect(module.PropertyGroups);
            var whereClass = GenerateWhere(whereParameters);
            var orderByClass = GenerateOrderBy(sortParameters, module.PropertyGroups.SelectMany(g => g.Properties));

            var codeUnit = SyntaxEx.CompilationUnit(
                SyntaxEx.NamespaceDeclaration(m_wiki.Namespace, selectClass, whereClass, orderByClass),
                "System", "System.Globalization", "System.Xml.Linq");

            m_wiki.Files.Add(m_typeNameBase, codeUnit);

            GenerateMethod(module, methodParameters, orderByClass != null);

            m_wiki.ModuleFinished();
        }

        private static IList<Parameter> RemoveAndReturnByNames(List<Parameter> parameters, params string[] names)
        {
            return parameters.RemoveAndReturn(p => names.Contains(p.Name));
        }

        private ClassDeclarationSyntax GenerateSelect(IEnumerable<PropertyGroup> propertyGroups)
        {
            var queryActionFile = m_wiki.Files[Wiki.Names.QueryAction];
            var queryActionClass = queryActionFile.SingleDescendant<ClassDeclarationSyntax>();

            var initializers =
                from pg in propertyGroups
                from p in pg.Properties
                group pg.Name by p.Name
                into g
                select new[] { SyntaxEx.Literal(g.Key), SyntaxEx.ArrayCreation(null, g.Select(SyntaxEx.Literal)) };

            var propsInitializer = SyntaxEx.ObjectCreation("Dictionary<string, string[]>", null, initializers);

            var propsField =
                SyntaxEx.FieldDeclaration(
                    new[] { SyntaxKind.PrivateKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword },
                    "IDictionary<string, string[]>", m_typeNameBase + "Props", propsInitializer);

            m_selectProps = propsField;

            m_wiki.Files[Wiki.Names.QueryAction] = queryActionFile.ReplaceNode(
                queryActionClass, queryActionClass.WithAdditionalMembers(propsField));

            var properties = propertyGroups.SelectMany(g => g.Properties).Distinct().ToArray();

            return SyntaxEx.ClassDeclaration(
                m_selectClassName, properties.Select(p => GenerateProperty(p.Name, p.Type, p.Nullable)))
                .WithPrivateConstructor()
                .WithAdditionalMembers(GenerateParseMethod(properties), GenerateToStringMethod(properties));
        }

        private MethodDeclarationSyntax GenerateParseMethod(IEnumerable<Property> properties)
        {
            var elementParameter = SyntaxEx.Parameter("XElement", "element");
            var wikiParameter = SyntaxEx.Parameter("WikiInfo", "wiki");
            var statements = new List<StatementSyntax>();

            var resultLocal = SyntaxEx.LocalDeclaration(
                "var", "result", SyntaxEx.ObjectCreation(m_selectClassName));

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
                    m_wiki.TypeManager.CreateConverter(
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
                new[] { SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword }, m_selectClassName, "Parse",
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

        private PropertyDeclarationSyntax GenerateProperty(string name, ParameterType type, bool nullable = false, string description = null)
        {
            var result = SyntaxEx.AutoPropertyDeclaration(
                new[] { SyntaxKind.PublicKeyword }, m_wiki.TypeManager.GetTypeName(type, name, nullable),
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

        private ClassDeclarationSyntax GenerateWhere(IEnumerable<Parameter> parameters)
        {
            var propertyDeclarations =
                parameters.Select(p => GenerateProperty(p.Name, p.Type, description: p.Description));

            return SyntaxEx.ClassDeclaration(m_whereClassName, propertyDeclarations)
                .WithPrivateConstructor();
        }

        private ClassDeclarationSyntax GenerateOrderBy(IEnumerable<Parameter> parameters, IEnumerable<Property> properties)
        {
            var propertyTypes = properties.Distinct().ToDictionary(p => p.Name, p => p.Type);

            var sortParameter = parameters.SingleOrDefault(p => p.Name == "sort");

            if (!parameters.Any(p => p.Name == "dir"))
                return null;

            IEnumerable<PropertyDeclarationSyntax> propertyDeclarations = null;

            if (sortParameter != null)
                propertyDeclarations =
                    ((EnumParameterType)sortParameter.Type).Values.Select(v => GenerateProperty(v, propertyTypes[v]));

            return SyntaxEx.ClassDeclaration(m_orderByClassName, propertyDeclarations)
                .WithPrivateConstructor();
        }

        private void GenerateMethod(Module module, IEnumerable<Parameter> methodParameters, bool sortable)
        {
            var queryActionFile = m_wiki.Files[Wiki.Names.QueryAction];
            var queryActionClass = queryActionFile.SingleDescendant<ClassDeclarationSyntax>();

            var queryTypePropertiesType = SyntaxEx.GenericName("QueryTypeProperties", m_selectClassName);

            var propertiesInitializer = SyntaxEx.ObjectCreation(
                queryTypePropertiesType,
                SyntaxEx.Literal(module.Name),
                SyntaxEx.Literal(module.Prefix),
                SyntaxEx.MemberAccess("QueryType", module.QueryType.ToString()),
                CreateTupleListExpression(
                    Wiki.QueryBaseParameters.Concat(
                        new TupleList<string, string>
                        { { module.QueryType.ToString().ToLowerInvariant(), module.Name } })),
                (NamedNode)m_selectProps, SyntaxEx.MemberAccess(m_selectClassName, "Parse"));

            var propertiesField =
                SyntaxEx.FieldDeclaration(
                    new[] { SyntaxKind.PrivateKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword },
                    queryTypePropertiesType, m_typeNameBase + "Properties", propertiesInitializer);

            var queryType =
                sortable
                    ? SyntaxEx.GenericName("WikiQuerySortable", m_whereClassName, m_orderByClassName, m_selectClassName)
                    : SyntaxEx.GenericName("WikiQuery", m_whereClassName, m_selectClassName);

            ExpressionSyntax queryParameters = SyntaxEx.Invocation(
                SyntaxEx.MemberAccess("QueryParameters", SyntaxEx.GenericName("Create", m_selectClassName)));

            var documentationElements = new List<XmlElementSyntax>();

            var summary = SyntaxEx.DocumentationSummary(module.Description);
            documentationElements.Add(summary);

            var parameters = new List<ParameterSyntax>();

            foreach (var methodParameter in methodParameters)
            {
                var parameter = SyntaxEx.Parameter(
                    m_wiki.TypeManager.GetTypeName(methodParameter), methodParameter.Name);

                parameters.Add(parameter);

                queryParameters = SyntaxEx.Invocation(
                    SyntaxEx.MemberAccess(queryParameters, "AddSingleValue"),
                    SyntaxEx.Literal(methodParameter.Name),
                    SyntaxEx.Invocation(SyntaxEx.MemberAccess(parameter, "ToQueryString")));

                var parameterDocumentation = SyntaxEx.DocumentationParameter(
                    methodParameter.Name, methodParameter.Description);

                documentationElements.Add(parameterDocumentation);
            }

            var queryCreation = SyntaxEx.ObjectCreation(
                queryType,
                SyntaxEx.ObjectCreation(
                    SyntaxEx.GenericName("QueryProcessor", m_selectClassName),
                    Syntax.IdentifierName("m_wiki"),
                    (NamedNode)propertiesField),
                queryParameters);

            var method = SyntaxEx.MethodDeclaration(
                new[] { SyntaxKind.PublicKeyword }, queryType, m_typeNameBase, parameters,
                SyntaxEx.Return(queryCreation))
                .WithLeadingTrivia(Syntax.Trivia(SyntaxEx.DocumentationComment(documentationElements)));

            m_wiki.Files[Wiki.Names.QueryAction] = queryActionFile.ReplaceNode(
                queryActionClass, queryActionClass.WithAdditionalMembers(propertiesField).WithAdditionalMembers(method));
        }

        private static ObjectCreationExpressionSyntax CreateTupleListExpression(IEnumerable<Tuple<string, string>> tupleList)
        {
            return SyntaxEx.ObjectCreation(
                "TupleList<string, string>", null,
                tupleList.Select(t => new[] { t.Item1, t.Item2 }.Select(SyntaxEx.Literal)));
        }

        private static string GetTypeNameBase(Module module)
        {
            var prefixes = new[] { "Api", "Query" };

            string name = module.ClassName;
            foreach (var prefix in prefixes)
            {
                if (name.StartsWith(prefix))
                    name = name.Substring(prefix.Length);
            }

            return name;
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
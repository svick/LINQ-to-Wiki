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

        public void Generate(ParamInfo paramInfo)
        {
            m_typeNameBase = GetTypeNameBase(paramInfo);
            m_selectClassName = m_typeNameBase + "Select";
            m_whereClassName = m_typeNameBase + "Where";
            m_orderByClassName = m_typeNameBase + "OrderBy";

            var parameters = paramInfo.Parameters.ToList();

            var methodParameters = RemoveAndReturnByNames(parameters, "title", "pageid");

            var sortParameters = RemoveAndReturnByNames(parameters, "sort", "dir");

            // don't belong anywhere, are used in a special way
            RemoveAndReturnByNames(parameters, "continue", "limit", "prop");

            var whereParameters = parameters;

            var selectClass = GenerateSelect(paramInfo.PropertyGroups);
            var whereClass = GenerateWhere(whereParameters);
            var orderByClass = GenerateOrderBy(sortParameters);

            var codeUnit = SyntaxEx.CompilationUnit(
                SyntaxEx.NamespaceDeclaration(m_wiki.Namespace, selectClass, whereClass, orderByClass),
                "System", "System.Globalization", "System.Xml.Linq");

            m_wiki.Files.Add(m_typeNameBase, codeUnit);

            GenerateMethod(paramInfo, methodParameters);
        }

        private static IList<Parameter> RemoveAndReturnByNames(List<Parameter> parameters, params string[] names)
        {
            return parameters.RemoveAndReturn(p => names.Contains(p.Name));
        }

        private ClassDeclarationSyntax GenerateSelect(IEnumerable<PropertyGroup> propertyGroups)
        {
            var queryActionFile = m_wiki.Files[Wiki.Names.QueryAction];
            var queryActionClass = queryActionFile.SingleDescendant<ClassDeclarationSyntax>();

            var initializerParameters = from g in propertyGroups
                                        from p in g.Properties
                                        select new[] { p.Name, g.Name } into names
                                        from name in names
                                        select SyntaxEx.Literal(name);

            var propsInitializer = SyntaxEx.ObjectCreation("SingleTypeDictionary<string>", initializerParameters);

            var propsField =
                SyntaxEx.FieldDeclaration(
                    new[] { SyntaxKind.PrivateKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword },
                    "IDictionary<string, string>", m_typeNameBase + "Props", propsInitializer);

            m_selectProps = propsField;

            m_wiki.Files[Wiki.Names.QueryAction] = queryActionFile.ReplaceNode(
                queryActionClass, queryActionClass.WithAdditionalMembers(propsField));

            var properties = propertyGroups.SelectMany(g => g.Properties).ToArray();

            var selectClass = SyntaxEx.ClassDeclaration(
                m_selectClassName, properties.Select(GenerateProperty));

            selectClass =
                selectClass.WithAdditionalMembers(
                    SyntaxEx.ConstructorDeclaration(
                        new[] { SyntaxKind.PrivateKeyword }, m_selectClassName, new ParameterSyntax[0],
                        new StatementSyntax[0]));

            selectClass = selectClass.WithAdditionalMembers(GenerateParseMethod(properties));

            return selectClass;
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
                var propertyAttributeLocal = SyntaxEx.LocalDeclaration(
                    "var", property.Name + "Attribute",
                    SyntaxEx.Invocation(
                        SyntaxEx.MemberAccess(elementParameter, "Attribute"),
                        SyntaxEx.Invocation(SyntaxEx.MemberAccess("XName", "Get"), SyntaxEx.Literal(property.Name))));

                statements.Add(propertyAttributeLocal);

                var assignment = SyntaxEx.Assignment(
                    SyntaxEx.MemberAccess(resultLocal, property.Name),
                    m_wiki.TypeManager.CreateConverter(
                        property, SyntaxEx.MemberAccess(propertyAttributeLocal, "Value"), (NamedNode)wikiParameter));

                var ifStatement = SyntaxEx.If(SyntaxEx.NotEquals((NamedNode)propertyAttributeLocal, SyntaxEx.NullLiteral()), assignment);
                statements.Add(ifStatement);
            }

            statements.Add(SyntaxEx.Return(resultLocal));

            return SyntaxEx.MethodDeclaration(
                new[] { SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword }, m_selectClassName, "Parse",
                new[] { elementParameter, wikiParameter }, statements);
        }

        private PropertyDeclarationSyntax GenerateProperty(Property property)
        {
            return SyntaxEx.AutoPropertyDeclaration(
                new[] { SyntaxKind.PublicKeyword }, m_wiki.TypeManager.GetTypeName(property), property.Name,
                SyntaxKind.PrivateKeyword);
        }

        private ClassDeclarationSyntax GenerateWhere(IEnumerable<Parameter> parameters)
        {
            var properties = parameters.Select(
                p =>
                SyntaxEx.AutoPropertyDeclaration(
                    new[] { SyntaxKind.PublicKeyword }, m_wiki.TypeManager.GetTypeName(p), p.Name));

            return SyntaxEx.ClassDeclaration(m_whereClassName, properties);
        }

        private ClassDeclarationSyntax GenerateOrderBy(IEnumerable<Parameter> parameters)
        {
            var orderByClass = SyntaxEx.ClassDeclaration(m_orderByClassName);

            // TODO

            return orderByClass;
        }

        private void GenerateMethod(ParamInfo paramInfo, IEnumerable<Parameter> methodParameters)
        {
            var queryActionFile = m_wiki.Files[Wiki.Names.QueryAction];
            var queryActionClass = queryActionFile.SingleDescendant<ClassDeclarationSyntax>();

            var queryTypePropertiesType = SyntaxEx.GenericName("QueryTypeProperties", m_selectClassName);

            var propertiesInitializer = SyntaxEx.ObjectCreation(
                queryTypePropertiesType, SyntaxEx.Literal(paramInfo.Prefix), SyntaxEx.Literal(paramInfo.Prefix),
                CreateTupleListExpression(
                    Wiki.QueryBaseParameters.Concat(new TupleList<string, string> { { "list", paramInfo.Name } })),
                (NamedNode)m_selectProps, SyntaxEx.MemberAccess(m_selectClassName, "Parse"));

            var propertiesField =
                SyntaxEx.FieldDeclaration(
                    new[] { SyntaxKind.PrivateKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword },
                    queryTypePropertiesType, m_typeNameBase + "Properties", propertiesInitializer);

            var queryType = SyntaxEx.GenericName("WikiQuery", m_whereClassName, m_orderByClassName, m_selectClassName);

            var methods = new List<MethodDeclarationSyntax>();

            foreach (var methodParameter in methodParameters)
            {
                var parameter = SyntaxEx.Parameter(
                    m_wiki.TypeManager.GetTypeName(methodParameter), methodParameter.Name);

                ExpressionSyntax queryParameters = SyntaxEx.Invocation(
                    SyntaxEx.MemberAccess("QueryParameters", SyntaxEx.GenericName("Create", m_selectClassName)));

                queryParameters = SyntaxEx.Invocation(
                    SyntaxEx.MemberAccess(queryParameters, "AddSingleValue"),
                    SyntaxEx.Literal(methodParameter.Name),
                    SyntaxEx.Invocation(SyntaxEx.MemberAccess(parameter, "ToString"))); // ToString won't work for Namespace

                var queryCreation = SyntaxEx.ObjectCreation(
                    queryType,
                    SyntaxEx.ObjectCreation(
                        SyntaxEx.GenericName("QueryProcessor", m_selectClassName),
                        Syntax.IdentifierName("m_wiki"),
                        (NamedNode)propertiesField),
                    queryParameters);

                var method = SyntaxEx.MethodDeclaration(
                    new[] { SyntaxKind.PublicKeyword }, queryType, m_typeNameBase, new[] { parameter },
                    SyntaxEx.Return(queryCreation));

                methods.Add(method);
            }

            m_wiki.Files[Wiki.Names.QueryAction] = queryActionFile.ReplaceNode(
                queryActionClass, queryActionClass.WithAdditionalMembers(propertiesField).WithAdditionalMembers(methods));
        }

        private static ObjectCreationExpressionSyntax CreateTupleListExpression(IEnumerable<Tuple<string, string>> tupleList)
        {
            return SyntaxEx.ObjectCreation(
                "SingleTypeTupleList<string>",
                tupleList.SelectMany(t => new[] { t.Item1, t.Item2 }).Select(SyntaxEx.Literal));
        }

        private static string GetTypeNameBase(ParamInfo paramInfo)
        {
            var prefixes = new[] { "Api", "Query" };

            string name = paramInfo.ClassName;
            foreach (var prefix in prefixes)
            {
                if (name.StartsWith(prefix))
                    name = name.Substring(prefix.Length);
            }

            return name;
        }
    }
}
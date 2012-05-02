using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Internals;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen.ModuleGenerators
{
    abstract class ModuleGeneratorBase
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

        protected ClassDeclarationSyntax GenerateClassForProperties(string className, IEnumerable<Property> properties, string baseType = null)
        {
            var propertiesArray = properties.ToArray();

            return SyntaxEx.ClassDeclaration(
                className, baseType == null ? null : SyntaxEx.ParseTypeName(baseType),
                propertiesArray.Select(p => GenerateProperty(p.Name, p.Type, p.Nullable)))
                .WithPrivateConstructor()
                .WithAdditionalMembers(
                    GenerateParseMethod(className, propertiesArray), GenerateToStringMethod(propertiesArray));
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
                var propertyValueLocal = SyntaxEx.LocalDeclaration(
                    "var", GetPropertyName(property.Name, false) + "Value", propertyValueAccess);

                statements.Add(propertyValueLocal);

                var valueValue = SyntaxEx.MemberAccess(propertyValueLocal, "Value");

                var assignment = SyntaxEx.Assignment(
                    SyntaxEx.MemberAccess(resultLocal, GetPropertyName(property.Name)),
                    Wiki.TypeManager.CreateConverter(
                        property, ClassNameBase, valueValue,
                        (NamedNode)wikiParameter));

                if (checkForNull)
                {
                    ExpressionSyntax condition = SyntaxEx.NotEquals((NamedNode)propertyValueLocal, SyntaxEx.NullLiteral());

                    var simpleType = property.Type as SimpleParameterType;
                    if (simpleType == null || (simpleType.Name != "string" && simpleType.Name != "boolean"))
                        condition = SyntaxEx.And(
                            condition, SyntaxEx.NotEquals(valueValue, SyntaxEx.Literal("")));

                    var ifStatement = SyntaxEx.If(condition, assignment);
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
                "; ", properties.Select((p, i) => string.Format("{0}: {{{1}}}", GetPropertyName(p.Name, false), i)));

            var parameters = new List<ExpressionSyntax> { SyntaxEx.Literal(formatString) };

            parameters.AddRange(properties.Select(p => Syntax.IdentifierName(GetPropertyName(p.Name))));

            var returnStatement = SyntaxEx.Return(SyntaxEx.Invocation(SyntaxEx.MemberAccess("string", "Format"), parameters));

            return SyntaxEx.MethodDeclaration(
                new[] { SyntaxKind.PublicKeyword, SyntaxKind.OverrideKeyword }, "string", "ToString", null,
                returnStatement);
        }

        protected PropertyDeclarationSyntax GenerateProperty(
            string name, ParameterType type, bool nullable = false, string description = null, bool multi = false)
        {
            var result = SyntaxEx.AutoPropertyDeclaration(
                new[] { SyntaxKind.PublicKeyword }, Wiki.TypeManager.GetTypeName(type, name, ClassNameBase, multi, nullable),
                GetPropertyName(name), SyntaxKind.PrivateKeyword);

            if (description != null)
                result = result.WithDocumentationSummary(description);

            return result;
        }

        private static string GetPropertyName(string name, bool identifier = true)
        {
            if (name == "*")
                return "value";
            if (name == "namespace")
                return "ns";
            if (name == "default")
                return "defaultvalue";
            if (name == "new")
                return identifier ? "@new" : "new";

            return name.Replace('-', '_');
        }

        protected void GenerateMethod(
            Module module, IEnumerable<Parameter> methodParameters, string resultClassName,
            FieldDeclarationSyntax propsField, string fileName, bool nullableParameters, SortType? sortType)
        {
            var propertiesField = CreatePropertiesField(module, resultClassName, propsField, sortType);

            ExpressionSyntax queryParameters = SyntaxEx.Invocation(
                SyntaxEx.MemberAccess("QueryParameters", SyntaxEx.GenericName("Create", resultClassName)));

            var queryParametersLocal = SyntaxEx.LocalDeclaration("var", "queryParameters", queryParameters);

            var documentationElements = new List<XmlElementSyntax>();

            var summary = SyntaxEx.DocumentationSummary(module.Description);
            documentationElements.Add(summary);

            var parameters = new List<ParameterSyntax>();
            var statements = new List<StatementSyntax>();

            statements.Add(queryParametersLocal);

            methodParameters = methodParameters.Where(p => !p.Deprecated)
                .OrderByDescending(p => p.Required);

            foreach (var methodParameter in methodParameters)
            {
                var nullable = nullableParameters && !methodParameter.Required;
                var typeName = Wiki.TypeManager.GetTypeName(methodParameter, ClassNameBase, nullable, false);
                var parameterName = GetPropertyName(methodParameter.Name);
                var parameter = SyntaxEx.Parameter(typeName, parameterName, nullable ? SyntaxEx.NullLiteral() : null);

                parameters.Add(parameter);

                ExpressionSyntax valueExpression = (NamedNode)parameter;

                if (nullable && typeName.EndsWith("?"))
                    valueExpression = SyntaxEx.MemberAccess(valueExpression, "Value");

                var queryParametersAssignment = SyntaxEx.Assignment(
                    queryParametersLocal, SyntaxEx.Invocation(
                        SyntaxEx.MemberAccess(queryParametersLocal, "AddSingleValue"),
                        SyntaxEx.Literal(methodParameter.Name),
                        SyntaxEx.Invocation(SyntaxEx.MemberAccess(valueExpression, "ToQueryString"))));
                
                if (nullable)
                {
                    var assignmentWithCheck = SyntaxEx.If(
                        SyntaxEx.NotEquals((NamedNode)parameter, SyntaxEx.NullLiteral()), queryParametersAssignment);

                    statements.Add(assignmentWithCheck);
                }
                else
                    statements.Add(queryParametersAssignment);

                var parameterDocumentation = SyntaxEx.DocumentationParameter(
                    parameterName, new System.Xml.Linq.XText(methodParameter.Description).ToString());

                documentationElements.Add(parameterDocumentation);
            }

            GenerateMethodBody(
                SyntaxEx.ObjectCreation(
                    SyntaxEx.GenericName("QueryProcessor", resultClassName),
                    Syntax.IdentifierName("m_wiki"),
                    (NamedNode)propertiesField), (NamedNode)queryParametersLocal, statements);

            var method = SyntaxEx.MethodDeclaration(
                new[] { SyntaxKind.PublicKeyword }, GenerateMethodResultType(), ClassNameBase, parameters, statements)
                .WithLeadingTrivia(Syntax.Trivia(SyntaxEx.DocumentationComment(documentationElements)));

            AddMembersToClass(fileName, propertiesField, method);
        }

        protected FieldDeclarationSyntax CreatePropertiesField(
            Module module, string resultClassName, FieldDeclarationSyntax propsField, SortType? sortType)
        {
            var queryTypePropertiesType = SyntaxEx.GenericName("QueryTypeProperties", resultClassName);

            var propertiesInitializer = SyntaxEx.ObjectCreation(
                queryTypePropertiesType,
                SyntaxEx.Literal(module.Name),
                SyntaxEx.Literal(module.Prefix),
                module.QueryType == null
                    ? (ExpressionSyntax)SyntaxEx.NullLiteral()
                    : SyntaxEx.MemberAccess("QueryType", module.QueryType.ToString()),
                sortType == null
                    ? (ExpressionSyntax)SyntaxEx.NullLiteral()
                    : SyntaxEx.MemberAccess("SortType", sortType.ToString()),
                CreateTupleListExpression(GetBaseParameters(module)),
                propsField == null ? (ExpressionSyntax)SyntaxEx.NullLiteral() : (NamedNode)propsField,
                resultClassName == "object"
                    ? (ExpressionSyntax)SyntaxEx.LambdaExpression("_", SyntaxEx.NullLiteral())
                    : SyntaxEx.MemberAccess(resultClassName, "Parse"));

            return SyntaxEx.FieldDeclaration(
                new[] { SyntaxKind.PrivateKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword },
                queryTypePropertiesType, ClassNameBase + "Properties", propertiesInitializer);
        }

        protected abstract IEnumerable<Tuple<string, string>> GetBaseParameters(Module module);

        protected abstract void GenerateMethodBody(
            ExpressionSyntax queryProcessor, ExpressionSyntax queryParameters, IList<StatementSyntax> statements);

        protected abstract TypeSyntax GenerateMethodResultType();

        private static ObjectCreationExpressionSyntax CreateTupleListExpression(IEnumerable<Tuple<string, string>> tupleList)
        {
            return SyntaxEx.ObjectCreation(
                "TupleList<string, string>", null,
                tupleList.Select(t => new[] { t.Item1, t.Item2 }.Select(SyntaxEx.Literal)));
        }

        protected FieldDeclarationSyntax CreatePropsField(IEnumerable<PropertyGroup> propertyGroups)
        {
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
                    "IDictionary<string, string[]>", ClassNameBase + "Props", propsInitializer);
            return propsField;
        }

        protected void AddMembersToClass(string fileName, params MemberDeclarationSyntax[] members)
        {
            var file = Wiki.Files[fileName];
            var @class = file.SingleDescendant<ClassDeclarationSyntax>();

            Wiki.Files[fileName] = file.ReplaceNode(@class, @class.WithAdditionalMembers(members));
        }
    }
}
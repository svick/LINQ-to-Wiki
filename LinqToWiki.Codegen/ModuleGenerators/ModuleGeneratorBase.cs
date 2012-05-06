using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Internals;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen.ModuleGenerators
{
    /// <summary>
    /// The base class for all module generators.
    /// </summary>
    abstract class ModuleGeneratorBase
    {
        /// <summary>
        /// Wiki, for which the module is generated.
        /// </summary>
        protected Wiki Wiki { get; private set; }

        /// <summary>
        /// Base name for all classes generated for the module.
        /// </summary>
        protected string ClassNameBase { get; private set; }

        protected ModuleGeneratorBase(Wiki wiki)
        {
            Wiki = wiki;
        }

        /// <summary>
        /// Generates code for the module.
        /// </summary>
        public void Generate(Module module)
        {
            if (module.PropertyGroups == null)
                return;

            ClassNameBase = module.Name;

            GenerateInternal(module);

            Wiki.ModuleFinished();
        }

        /// <summary>
        /// Actually generates code specific to the module.
        /// </summary>
        protected abstract void GenerateInternal(Module module);

        /// <summary>
        /// Creates a class for a set of properties.
        /// </summary>
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

        /// <summary>
        /// Creates a method that parses an XML element returned from the API by setting creating an instance
        /// of the given class and setting its properties.
        /// </summary>
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

        /// <summary>
        /// Creates an overide of the <see cref="object.ToString"/> method for given properties.
        /// </summary>
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

        /// <summary>
        /// Generates a single property.
        /// </summary>
        protected PropertyDeclarationSyntax GenerateProperty(
            string name, ParameterType type, bool nullable = false, string description = null, bool multi = false)
        {
            var result = SyntaxEx.AutoPropertyDeclaration(
                new[] { SyntaxKind.PublicKeyword },
                Wiki.TypeManager.GetTypeName(type, FixPropertyName(name), ClassNameBase, multi, nullable),
                GetPropertyName(name), SyntaxKind.PrivateKeyword);

            if (description != null)
                result = result.WithDocumentationSummary(description);

            return result;
        }

        /// <summary>
        /// Fixes the property name, so that it can be used as a base for type name.
        /// </summary>
        private static string FixPropertyName(string name)
        {
            if (name == "*")
                return "value";

            return name;
        }

        /// <summary>
        /// Fixes the property name, so that it is a valid property name in C#.
        /// </summary>
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

        /// <summary>
        /// Creates an entry method, that is used to execute query for normal modules
        /// or that can be used as a base for a query for query modules.
        /// </summary>
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
            IList<StatementSyntax> statements = new List<StatementSyntax>();

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

            statements = GenerateMethodBody(
                SyntaxEx.ObjectCreation(
                    SyntaxEx.GenericName("QueryProcessor", resultClassName),
                    Syntax.IdentifierName("m_wiki"),
                    (NamedNode)propertiesField), (NamedNode)queryParametersLocal, statements);

            var modifiers = new List<SyntaxKind> { SyntaxKind.PublicKeyword };
            if (statements == null)
                modifiers.Add(SyntaxKind.AbstractKeyword);

            var method = SyntaxEx.MethodDeclaration(
                modifiers, GenerateMethodResultType(), ClassNameBase, parameters, statements)
                .WithLeadingTrivia(Syntax.Trivia(SyntaxEx.DocumentationComment(documentationElements)));

            AddMembersToClass(fileName, propertiesField, method);
        }

        /// <summary>
        /// Creates a field that holds <see cref="QueryTypeProperties{T}"/> for the module.
        /// </summary>
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

        /// <summary>
        /// Gets parameters that are used in all queries for this module.
        /// </summary>
        protected abstract IEnumerable<Tuple<string, string>> GetBaseParameters(Module module);

        /// <summary>
        /// Creates the body of the method created in <see cref="GenerateMethod"/>.
        /// </summary>
        protected abstract IList<StatementSyntax> GenerateMethodBody(
            ExpressionSyntax queryProcessor, ExpressionSyntax queryParameters, IList<StatementSyntax> commonStatements);

        /// <summary>
        /// Returns the return type of the method from <see cref="GenerateMethod"/>.
        /// </summary>
        protected abstract TypeSyntax GenerateMethodResultType();

        /// <summary>
        /// Creates an expression that creates the <see cref="LinqToWiki.Collections.TupleList{T1,T2}"/>
        /// that is passed as a parameter.
        /// </summary>
        private static ObjectCreationExpressionSyntax CreateTupleListExpression(IEnumerable<Tuple<string, string>> tupleList)
        {
            return SyntaxEx.ObjectCreation(
                "TupleList<string, string>", null,
                tupleList.Select(t => new[] { t.Item1, t.Item2 }.Select(SyntaxEx.Literal)));
        }

        /// <summary>
        /// Creates a field that holds information about property groups for the given module.
        /// </summary>
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

        /// <summary>
        /// Adds members to the only class in a given file.
        /// </summary>
        protected void AddMembersToClass(string fileName, params MemberDeclarationSyntax[] members)
        {
            var file = Wiki.Files[fileName];
            var classDeclaration = file.SingleDescendant<ClassDeclarationSyntax>();

            Wiki.Files[fileName] = file.ReplaceNode(classDeclaration, classDeclaration.WithAdditionalMembers(members));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Collections;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen
{
    class ListModuleGenerator : ModuleGeneratorBase
    {
        private string m_selectClassName;
        private string m_whereClassName;
        private string m_orderByClassName;

        private FieldDeclarationSyntax m_selectProps;
        private GenericNameSyntax m_queryType;

        public ListModuleGenerator(Wiki wiki)
            : base(wiki)
        {}

        protected override void GenerateInternal(Module module)
        {
            m_selectClassName = ClassNameBase + "Select";
            m_whereClassName = ClassNameBase + "Where";
            m_orderByClassName = ClassNameBase + "OrderBy";

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
                SyntaxEx.NamespaceDeclaration(Wiki.Namespace, selectClass, whereClass, orderByClass),
                "System", "System.Globalization", "System.Xml.Linq");

            Wiki.Files.Add(ClassNameBase, codeUnit);

            m_queryType =
                orderByClass == null
                    ? SyntaxEx.GenericName("WikiQuery", m_whereClassName, m_selectClassName)
                    : SyntaxEx.GenericName("WikiQuerySortable", m_whereClassName, m_orderByClassName, m_selectClassName);

            SortType? sortType = null;

            var dirParameter = sortParameters.SingleOrDefault(p => p.Name == "dir");

            if (dirParameter != null)
            {
                var type = (EnumParameterType)dirParameter.Type;
                if (type.Values.Any(x => x == "ascending"))
                    sortType = SortType.Ascending;
                else if (type.Values.Any(x => x == "newer"))
                    sortType = SortType.Newer;
            }

            GenerateMethod(
                module, methodParameters, m_selectClassName, m_selectProps, Wiki.Names.QueryAction, false, sortType);
        }

        private static IList<Parameter> RemoveAndReturnByNames(List<Parameter> parameters, params string[] names)
        {
            return parameters.RemoveAndReturn(p => names.Contains(p.Name));
        }

        private ClassDeclarationSyntax GenerateSelect(IEnumerable<PropertyGroup> propertyGroups)
        {
            var queryActionFile = Wiki.Files[Wiki.Names.QueryAction];
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
                    "IDictionary<string, string[]>", ClassNameBase + "Props", propsInitializer);

            m_selectProps = propsField;

            Wiki.Files[Wiki.Names.QueryAction] = queryActionFile.ReplaceNode(
                queryActionClass, queryActionClass.WithAdditionalMembers(propsField));

            var properties = propertyGroups.SelectMany(g => g.Properties).Distinct();

            return GenerateClassForProperties(m_selectClassName, properties);
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

        protected override IEnumerable<Tuple<string, string>> GetBaseParameters(Module module)
        {
            return Wiki.QueryBaseParameters.Concat(
                new TupleList<string, string> { { module.QueryType.ToString().ToLowerInvariant(), module.Name } });
        }

        protected override ExpressionSyntax GenerateMethodResult(ExpressionSyntax queryProcessor, ExpressionSyntax queryParameters)
        {
            return SyntaxEx.ObjectCreation(m_queryType, queryProcessor, queryParameters);
        }

        protected override TypeSyntax GenerateMethodResultType()
        {
            return m_queryType;
        }
    }
}
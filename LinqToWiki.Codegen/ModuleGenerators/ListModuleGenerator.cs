using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Collections;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen.ModuleGenerators
{
    class ListModuleGenerator : ModuleGeneratorBase
    {
        protected string SelectClassName { get; private set; }
        protected string WhereClassName { get; private set; }
        protected string OrderByClassName { get; private set; }

        protected virtual string MethodClassName
        {
            get { return Wiki.Names.QueryAction; }
        }

        protected FieldDeclarationSyntax SelectProps { get; private set; }
        private GenericNameSyntax m_queryType;

        public ListModuleGenerator(Wiki wiki)
            : base(wiki)
        {}

        protected override void GenerateInternal(Module module)
        {
            SelectClassName = ClassNameBase + "Select";
            WhereClassName = ClassNameBase + "Where";
            OrderByClassName = ClassNameBase + "OrderBy";

            var parameters = module.Parameters.ToList();

            var sortParameters = RemoveAndReturnByNames(parameters, "sort", "dir");

            var methodParameters = parameters.RemoveAndReturn(p => p.Required);

            // don't belong anywhere, are used in a special way
            RemoveAndReturnByNames(parameters, "continue", "offset", "limit", "prop");

            var whereParameters = parameters;

            var selectClass = GenerateSelect(module.PropertyGroups);
            var whereClass = GenerateWhere(whereParameters);
            var orderByClass = GenerateOrderBy(sortParameters, module.PropertyGroups.SelectMany(g => g.Properties));

            var codeUnit = SyntaxEx.CompilationUnit(
                SyntaxEx.NamespaceDeclaration(Wiki.Namespace, selectClass, whereClass, orderByClass),
                "System", "LinqToWiki.Collections", "System.Globalization", "System.Xml.Linq");

            Wiki.Files.Add(ClassNameBase, codeUnit);

            string queryTypeName = "WikiQuery";
            var queryTypeGenericParameters = new List<string> { WhereClassName, SelectClassName };

            if (orderByClass != null)
            {
                queryTypeName += "Sortable";
                queryTypeGenericParameters.Insert(1, OrderByClassName);
            }

            if (module.Generator)
            {
                queryTypeName += "Generator";
                queryTypeGenericParameters.Insert(0, Wiki.Names.Page);
            }

            m_queryType = SyntaxEx.GenericName(queryTypeName, queryTypeGenericParameters);

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

            GenerateMethod(module, methodParameters, SelectClassName, SelectProps, MethodClassName, false, sortType);
        }

        private static IList<Parameter> RemoveAndReturnByNames(List<Parameter> parameters, params string[] names)
        {
            return parameters.RemoveAndReturn(p => names.Contains(p.Name));
        }

        private ClassDeclarationSyntax GenerateSelect(IEnumerable<PropertyGroup> propertyGroups)
        {
            propertyGroups = propertyGroups.Where(pg => pg.Name != null).ToArray();

            var propsField = CreatePropsField(propertyGroups);

            SelectProps = propsField;

            AddMembersToClass(MethodClassName, propsField);

            var properties = propertyGroups.SelectMany(g => g.Properties).Distinct();

            return GenerateClassForProperties(SelectClassName, properties);
        }

        private ClassDeclarationSyntax GenerateWhere(IEnumerable<Parameter> parameters)
        {
            var propertyDeclarations =
                parameters.Select(p => GenerateProperty(p.Name, p.Type, multi: p.Multi, description: p.Description));

            return SyntaxEx.ClassDeclaration(WhereClassName, propertyDeclarations)
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

            return SyntaxEx.ClassDeclaration(OrderByClassName, propertyDeclarations)
                .WithPrivateConstructor();
        }

        protected override IEnumerable<Tuple<string, string>> GetBaseParameters(Module module)
        {
            return Wiki.QueryBaseParameters.Concat(
                new TupleList<string, string> { { module.QueryType.ToString().ToLowerInvariant(), module.Name } });
        }

        protected override void GenerateMethodBody(ExpressionSyntax queryProcessor, ExpressionSyntax queryParameters, IList<StatementSyntax> statements)
        {
            statements.Add(SyntaxEx.Return(SyntaxEx.ObjectCreation(m_queryType, queryProcessor, queryParameters)));
        }

        protected override TypeSyntax GenerateMethodResultType()
        {
            return m_queryType;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Collections;
using LinqToWiki.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LinqToWiki.Codegen.ModuleGenerators
{
    /// <summary>
    /// Generates code for <see cref="LinqToWiki.Internals.QueryType.List"/> query modules,
    /// or <see cref="LinqToWiki.Internals.QueryType.Meta"/> query modules
    /// that return a list of results.
    /// </summary>
    class QueryModuleGenerator : ModuleGeneratorBase
    {
        private string m_selectClassName;
        private string m_whereClassName;
        private string m_orderByClassName;

        /// <summary>
        /// Name of the class that contains the entry method for this module.
        /// </summary>
        protected virtual string MethodClassName
        {
            get { return Wiki.Names.QueryAction; }
        }

        private FieldDeclarationSyntax m_selectProps;
        private GenericNameSyntax m_queryType;

        public QueryModuleGenerator(Wiki wiki)
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
            RemoveAndReturnByNames(parameters, "continue", "offset", "limit", "prop");

            var whereParameters = parameters;

            var selectClass = GenerateSelect(module.PropertyGroups, module.Name == "revisions");
            var whereClass = GenerateWhere(whereParameters);
            var orderByClass = GenerateOrderBy(sortParameters, module.PropertyGroups.SelectMany(g => g.Properties));

            var codeUnit = SyntaxEx.CompilationUnit(
                SyntaxEx.NamespaceDeclaration(Wiki.EntitiesNamespace, selectClass, whereClass, orderByClass),
                "System", "System.Globalization", "System.Xml.Linq", "LinqToWiki", "LinqToWiki.Collections",
                "LinqToWiki.Internals");

            Wiki.Files.Add(ClassNameBase, codeUnit);

            string queryTypeName = "WikiQuery";
            var queryTypeGenericParameters = new List<string> { m_whereClassName, m_selectClassName };

            if (orderByClass != null)
            {
                queryTypeName += "Sortable";
                queryTypeGenericParameters.Insert(1, m_orderByClassName);
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

            GenerateMethod(module, methodParameters, m_selectClassName, m_selectProps, MethodClassName, false, sortType);
        }

        /// <summary>
        /// Removes parameters with the given names from the list and returns them in a new list.
        /// </summary>
        private static IList<Parameter> RemoveAndReturnByNames(List<Parameter> parameters, params string[] names)
        {
            return parameters.RemoveAndReturn(p => names.Contains(p.Name));
        }

        /// <summary>
        /// Creates class that used in the <c>select</c> clause.
        /// </summary>
        private ClassDeclarationSyntax GenerateSelect(IEnumerable<PropertyGroup> propertyGroups, bool first)
        {
            propertyGroups = propertyGroups.Where(pg => pg.Name != null).ToArray();

            var propsField = CreatePropsField(propertyGroups);

            m_selectProps = propsField;

            AddMembersToClass(MethodClassName, propsField);

            var properties = propertyGroups.SelectMany(g => g.Properties).Distinct();

            return GenerateClassForProperties(m_selectClassName, properties, first ? "IFirst" : null);
        }

        /// <summary>
        /// Creates class that is used in the <c>where</c> clause.
        /// </summary>
        private ClassDeclarationSyntax GenerateWhere(IEnumerable<Parameter> parameters)
        {
            var propertyDeclarations =
                parameters.Select(p => GenerateProperty(p.Name, p.Type, multi: p.Multi, description: p.Description));

            return SyntaxEx.ClassDeclaration(m_whereClassName, propertyDeclarations)
                .AddPrivateConstructor();
        }

        /// <summary>
        /// Creates class that is ised in the <c>orderby</c> clause.
        /// Returns <c>null</c>, if the module doesn't support sorting.
        /// </summary>
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
                .AddPrivateConstructor();
        }

        protected override IEnumerable<Tuple<string, string>> GetBaseParameters(Module module)
        {
            return Wiki.QueryBaseParameters.Concat(
                new TupleList<string, string> { { module.QueryType.ToString().ToLowerInvariant(), module.Name } });
        }

        protected override IList<StatementSyntax> GenerateMethodBody(
            ExpressionSyntax queryProcessor, ExpressionSyntax queryParameters, IList<StatementSyntax> statements)
        {
            statements.Add(SyntaxEx.Return(SyntaxEx.ObjectCreation(m_queryType, queryProcessor, queryParameters)));
            return statements;
        }

        protected override TypeSyntax GenerateMethodResultType()
        {
            return m_queryType;
        }
    }
}
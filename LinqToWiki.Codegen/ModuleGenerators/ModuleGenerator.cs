using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Collections;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen.ModuleGenerators
{
    class ModuleGenerator : ModuleGeneratorBase
    {
        protected string ResultClassName { get; private set; }

        private bool m_listResult;

        public ModuleGenerator(Wiki wiki)
            : base(wiki)
        {}

        protected override void GenerateInternal(Module module)
        {
            ResultClassName = ClassNameBase + "Result";

            var resultType = GenerateResultClass(GetPropertyGroups(module));

            var codeUnit = SyntaxEx.CompilationUnit(
                SyntaxEx.NamespaceDeclaration(Wiki.EntitiesNamespace, resultType),
                "System", "System.Globalization", "System.Xml.Linq");

            Wiki.Files.Add(ClassNameBase, codeUnit);

            GenerateMethod(module);
        }

        protected virtual IEnumerable<PropertyGroup> GetPropertyGroups(Module module)
        {
            return module.PropertyGroups;
        }

        protected virtual void GenerateMethod(Module module)
        {
            GenerateMethod(module, module.Parameters, ResultClassName, null, Wiki.Names.Wiki, true, null);
        }

        protected virtual ClassDeclarationSyntax GenerateResultClass(IEnumerable<PropertyGroup> propertyGroups)
        {
            m_listResult = propertyGroups.Any(g => g.Name != null);

            var propertyGroup =
                m_listResult
                    ? propertyGroups.Single(g => g.Name == string.Empty)
                    : propertyGroups.Single(g => g.Name == null);

            return GenerateClassForProperties(ResultClassName, propertyGroup.Properties);
        }

        protected override IEnumerable<Tuple<string, string>> GetBaseParameters(Module module)
        {
            return new TupleList<string, string> { { "action", module.Name } };
        }

        protected override void GenerateMethodBody(ExpressionSyntax queryProcessor, ExpressionSyntax queryParameters, IList<StatementSyntax> statements)
        {
            statements.Add(
                SyntaxEx.Return(
                    SyntaxEx.Invocation(
                        SyntaxEx.MemberAccess(queryProcessor, m_listResult ? "ExecuteList" : "ExecuteSingle"),
                        queryParameters)));
        }

        protected override TypeSyntax GenerateMethodResultType()
        {
            var resultType = SyntaxEx.ParseTypeName(ResultClassName);

            if (m_listResult)
                resultType = SyntaxEx.GenericName("IEnumerable", resultType);

            return resultType;
        }
    }
}
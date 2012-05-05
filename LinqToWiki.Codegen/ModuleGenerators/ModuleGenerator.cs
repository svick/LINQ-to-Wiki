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
        private bool m_voidResult;

        public ModuleGenerator(Wiki wiki)
            : base(wiki)
        {}

        protected override void GenerateInternal(Module module)
        {
            ResultClassName = ClassNameBase + "Result";

            var resultType = GenerateResultClass(GetPropertyGroups(module));

            if (resultType == null)
            {
                ResultClassName = "object";
                m_voidResult = true;
            }
            else
            {
                var codeUnit = SyntaxEx.CompilationUnit(
                    SyntaxEx.NamespaceDeclaration(Wiki.EntitiesNamespace, resultType),
                    "System", "System.Linq", "System.Globalization", "System.Xml.Linq", "LinqToWiki",
                    "LinqToWiki.Internals");

                Wiki.Files.Add(ClassNameBase, codeUnit);
            }

            m_listResult = module.ListResult;

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
            var propertyGroup = propertyGroups.SingleOrDefault(g => g.Name == string.Empty);

            if (propertyGroup == null)
                return null;

            return GenerateClassForProperties(ResultClassName, propertyGroup.Properties);
        }

        protected override IEnumerable<Tuple<string, string>> GetBaseParameters(Module module)
        {
            return new TupleList<string, string> { { "action", module.Name } };
        }

        protected override IList<StatementSyntax> GenerateMethodBody(
            ExpressionSyntax queryProcessor, ExpressionSyntax queryParameters, IList<StatementSyntax> statements)
        {
            var invocation = SyntaxEx.Invocation(
                SyntaxEx.MemberAccess(queryProcessor, m_listResult ? "ExecuteList" : "ExecuteSingle"), queryParameters);

            var statement =
                m_voidResult
                    ? (StatementSyntax)Syntax.ExpressionStatement(invocation)
                    : SyntaxEx.Return(invocation);

            statements.Add(statement);
            return statements;
        }

        protected override TypeSyntax GenerateMethodResultType()
        {
            if (m_voidResult)
                return SyntaxEx.ParseTypeName("void");

            var resultType = SyntaxEx.ParseTypeName(ResultClassName);

            if (m_listResult)
                resultType = SyntaxEx.GenericName("IEnumerable", resultType);

            return resultType;
        }
    }
}
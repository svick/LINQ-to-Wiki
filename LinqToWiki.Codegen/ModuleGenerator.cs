using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Collections;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen
{
    public class ModuleGenerator : ModuleGeneratorBase
    {
        private string m_resultClassName;

        public ModuleGenerator(Wiki wiki)
            : base(wiki)
        {}

        protected override void GenerateInternal(Module module)
        {
            m_resultClassName = ClassNameBase + "Result";

            var resultType = GenerateResultClass(module.PropertyGroups);

            var codeUnit = SyntaxEx.CompilationUnit(
                SyntaxEx.NamespaceDeclaration(Wiki.Namespace, resultType),
                "System", "System.Globalization", "System.Xml.Linq");

            Wiki.Files.Add(ClassNameBase, codeUnit);

            GenerateMethod(module, module.Parameters, m_resultClassName, null, Wiki.Names.Wiki, true, null);
        }

        private ClassDeclarationSyntax GenerateResultClass(IEnumerable<PropertyGroup> propertyGroups)
        {
            if (propertyGroups.Any(g => g.Name != null))
                throw new NotImplementedException();

            var rootPropertyGroup = propertyGroups.Single(g => g.Name == null);

            return GenerateClassForProperties(m_resultClassName, rootPropertyGroup.Properties);
        }

        protected override IEnumerable<Tuple<string, string>> GetBaseParameters(Module module)
        {
            return new TupleList<string, string> { { "action", module.Name } };
        }

        protected override ExpressionSyntax GenerateMethodResult(ExpressionSyntax queryProcessor, ExpressionSyntax queryParameters)
        {
            return SyntaxEx.Invocation(SyntaxEx.MemberAccess(queryProcessor, "ExecuteSingle"), queryParameters);
        }

        protected override TypeSyntax GenerateMethodResultType()
        {
            return SyntaxEx.ParseTypeName(m_resultClassName);
        }
    }
}
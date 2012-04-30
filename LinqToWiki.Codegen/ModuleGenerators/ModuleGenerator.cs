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
            if (propertyGroups.Any(g => g.Name != null))
                throw new NotSupportedException();

            var rootPropertyGroup = propertyGroups.Single(g => g.Name == null);

            return GenerateClassForProperties(ResultClassName, rootPropertyGroup.Properties);
        }

        protected override IEnumerable<Tuple<string, string>> GetBaseParameters(Module module)
        {
            return new TupleList<string, string> { { "action", module.Name } };
        }

        protected override void GenerateMethodBody(ExpressionSyntax queryProcessor, ExpressionSyntax queryParameters, IList<StatementSyntax> statements)
        {
            statements.Add(
                SyntaxEx.Return(
                    SyntaxEx.Invocation(SyntaxEx.MemberAccess(queryProcessor, "ExecuteSingle"), queryParameters)));
        }

        protected override TypeSyntax GenerateMethodResultType()
        {
            return SyntaxEx.ParseTypeName(ResultClassName);
        }
    }
}
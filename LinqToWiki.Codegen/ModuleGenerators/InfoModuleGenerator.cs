using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Codegen.ModuleInfo;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen.ModuleGenerators
{
    public class InfoModuleGenerator : ModuleGenerator
    {
        public InfoModuleGenerator(Wiki wiki)
            : base(wiki)
        {}

        protected override ClassDeclarationSyntax GenerateResultClass(IEnumerable<PropertyGroup> propertyGroups)
        {
            return GenerateClassForProperties(ResultClassName, propertyGroups.SelectMany(g => g.Properties));
        }

        protected override void GenerateMethod(Module module)
        {
            GenerateMethod(
                module, module.Parameters.Where(p => p.Name != "continue" && p.Name != "prop"),
                ResultClassName, null, Wiki.Names.Page, true, null, true);
        }
    }
}
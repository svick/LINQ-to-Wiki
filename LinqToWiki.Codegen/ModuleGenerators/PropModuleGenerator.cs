using System;
using System.Collections.Generic;
using LinqToWiki.Codegen.ModuleInfo;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen.ModuleGenerators
{
    public class PropModuleGenerator : ModuleGeneratorBase
    {
        public PropModuleGenerator(Wiki wiki)
            : base(wiki)
        {}

        protected override void GenerateInternal(Module module)
        {
            
        }

        protected override IEnumerable<Tuple<string, string>> GetBaseParameters(Module module)
        {
            throw new NotImplementedException();
        }

        protected override ExpressionSyntax GenerateMethodResult(ExpressionSyntax queryProcessor, ExpressionSyntax queryParameters)
        {
            throw new NotImplementedException();
        }

        protected override TypeSyntax GenerateMethodResultType()
        {
            throw new NotImplementedException();
        }
    }
}
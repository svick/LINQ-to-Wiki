using System;
using System.Collections.Generic;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Collections;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen.ModuleGenerators
{
    class PropModuleGenerator : ListModuleGenerator
    {
        public PropModuleGenerator(Wiki wiki)
            : base(wiki)
        {}

        protected override string MethodClassName
        {
            get { return Wiki.Names.Page; }
        }

        protected override IEnumerable<Tuple<string, string>> GetBaseParameters(Module module)
        {
            return new TupleList<string, string>();
        }

        protected override IList<StatementSyntax> GenerateMethodBody(
            ExpressionSyntax queryProcessor, ExpressionSyntax queryParameters, IList<StatementSyntax> statements)
        {
            return null;
        }
    }
}
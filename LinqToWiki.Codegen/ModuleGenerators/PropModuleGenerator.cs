using System;
using System.Collections.Generic;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Collections;

namespace LinqToWiki.Codegen.ModuleGenerators
{
    class PropModuleGenerator : ListModuleGenerator
    {
        public PropModuleGenerator(Wiki wiki)
            : base(wiki)
        {}

        protected override IEnumerable<Tuple<string, string>> GetBaseParameters(Module module)
        {
            return new TupleList<string, string>();
        }
    }
}
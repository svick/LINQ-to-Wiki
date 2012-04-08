using System;
using System.IO;
using System.Linq;
using System.Text;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Parameters;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen
{
    public class Wiki
    {
        private static class ClassNames
        {
            public const string Wiki = "Wiki";
            public const string WikiInfo = "WikiInfo";
            public const string QueryAction = "QueryAction";
        }

        private readonly string m_ns;

        private readonly QueryProcessor<ParamInfo> m_processor;

        private readonly TupleList<string, CompilationUnitSyntax> m_files = new TupleList<string, CompilationUnitSyntax>();

        private const string Extension = ".cs";

        public Wiki(string baseUri, string apiPath, string ns = null)
        {
            m_ns = ns ?? "LinqToWiki";

            m_processor = new QueryProcessor<ParamInfo>(
                new WikiInfo(baseUri, apiPath),
                new QueryTypeProperties<ParamInfo>(
                    "", "module",
                    new TupleList<string, string> { { "action", "paraminfo" } },
                    null, 
                    ParamInfo.Parse));

            CreateQueryActionClass();
            CreateWikiClass();
        }

        private void CreateWikiClass()
        {
            var queryProperty = SyntaxEx.AutoPropertyDeclaration(
                new[] { SyntaxKind.PublicKeyword }, ClassNames.QueryAction, "Query", SyntaxKind.PrivateKeyword);

            var baseUriParameter = SyntaxEx.Parameter("string", "baseUri");
            var apiPathParameter = SyntaxEx.Parameter("string", "apiPath");

            var queryAssignment = SyntaxEx.Assignment(
                queryProperty,
                SyntaxEx.ObjectCreation(
                    ClassNames.QueryAction,
                    SyntaxEx.ObjectCreation(
                        ClassNames.WikiInfo, (NamedNode)baseUriParameter, (NamedNode)apiPathParameter)));

            var ctorWithParameters = SyntaxEx.ConstructorDeclaration(
                new[] { SyntaxKind.PublicKeyword }, ClassNames.Wiki,
                new[] { baseUriParameter, apiPathParameter },
                new StatementSyntax[] { queryAssignment });

            var ctorWithoutParameters = SyntaxEx.ConstructorDeclaration(
                new[] { SyntaxKind.PublicKeyword }, ClassNames.Wiki, new ParameterSyntax[0], new StatementSyntax[0],
                SyntaxEx.ThisConstructorInitializer(SyntaxEx.NullLiteral(), SyntaxEx.NullLiteral()));

            var wikiClass = SyntaxEx.ClassDeclaration(
                ClassNames.Wiki, queryProperty, ctorWithParameters, ctorWithoutParameters);

            m_files.Add(ClassNames.Wiki, SyntaxEx.CompilationUnit(SyntaxEx.NamespaceDeclaration(m_ns, wikiClass)));
        }

        private void CreateQueryActionClass()
        {
            var wikiField = SyntaxEx.FieldDeclaration(
                new[] { SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword }, ClassNames.WikiInfo, "m_wiki");

            var wikiParameter = SyntaxEx.Parameter(ClassNames.WikiInfo, "wiki");

            var ctor = SyntaxEx.ConstructorDeclaration(
                new[] { SyntaxKind.InternalKeyword }, ClassNames.QueryAction,
                new[] { wikiParameter },
                new StatementSyntax[] { SyntaxEx.Assignment(wikiField, wikiParameter) });

            var tupleListStringString = "TupleList<string, string>";

            // Roslyn doesn't support collection initializers yet
            var queryBaseParametersInitializer =
                Syntax.ParseExpression(@"new TupleList<string, string>(new Tuple<string, string>[] { Tuple.Create(""action"", ""query"") })");

            var queryBaseParametersField =
                SyntaxEx.FieldDeclaration(
                    new[] { SyntaxKind.PrivateKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword },
                    tupleListStringString, "QueryBaseParameters", queryBaseParametersInitializer);

            var queryActionClass = SyntaxEx.ClassDeclaration(ClassNames.QueryAction, wikiField, ctor, queryBaseParametersField);

            m_files.Add(
                ClassNames.QueryAction,
                SyntaxEx.CompilationUnit(SyntaxEx.NamespaceDeclaration(m_ns, queryActionClass), "System"));
        }

        public void AddQueryModule(string moduleName)
        {
            var paramInfo = m_processor
                .Execute(QueryParameters.Create<ParamInfo>().AddSingleValue("querymodules", moduleName))
                .Single();

            AddModule(paramInfo);
        }

        private void AddModule(ParamInfo paramInfo)
        {
            
        }

        public void Compile()
        {
            var compilation = Compilation.Create(
                "foo.dll", new CompilationOptions(assemblyKind: AssemblyKind.DynamicallyLinkedLibrary))
                .AddSyntaxTrees(m_files.Select(f => SyntaxTree.Create(f.Item1 + Extension, f.Item2)))
                .AddReferences(
                    new AssemblyFileReference(typeof(object).Assembly.Location),
                    new AssemblyFileReference(typeof(WikiInfo).Assembly.Location));

            var result = compilation.Emit(File.OpenWrite(@"C:\Temp\foo.dll"));

            foreach (var diagnostic in result.Diagnostics)
            {
                Console.WriteLine(diagnostic);
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            bool first = true;

            foreach (var file in m_files)
            {
                if (first)
                    first = false;
                else
                    builder.AppendLine();

                builder.AppendLine(file.Item1 + Extension + ':');
                builder.AppendLine(file.Item2.Format().ToString());
            }

            return builder.ToString();
        }
    }
}
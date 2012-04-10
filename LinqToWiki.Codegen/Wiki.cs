using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Collections;
using LinqToWiki.Parameters;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen
{
    public class Wiki
    {
        internal static class Names
        {
            public const string Wiki = "Wiki";
            public const string WikiInfo = "WikiInfo";
            public const string QueryAction = "QueryAction";
            public const string Enums = "Enums";
        }

        private readonly QueryProcessor<ParamInfo> m_processor;

        private const string Extension = ".cs";

        private string[] m_moduleNames;
        private string[] m_queryModuleNames;

        internal string Namespace { get; private set; }

        internal TupleList<string, CompilationUnitSyntax> Files { get; private set; }

        internal TypeManager TypeManager { get; private set; }

        internal static readonly TupleList<string, string> QueryBaseParameters =
            new TupleList<string, string> { { "action", "query" } };

        public Wiki(string baseUri, string apiPath, string ns = null)
        {
            Files = new TupleList<string, CompilationUnitSyntax>();
            TypeManager = new TypeManager(this);

            Namespace = ns ?? "LinqToWiki";

            m_processor = new QueryProcessor<ParamInfo>(
                new WikiInfo(baseUri, apiPath),
                new QueryTypeProperties<ParamInfo>(
                    "", "module",
                    new TupleList<string, string> { { "action", "paraminfo" } },
                    null, 
                    ParamInfo.Parse));

            CreateWikiClass();
            CreateQueryActionClass();
            CreateEnumsFile();
        }

        private void CreateQueryActionClass()
        {
            var wikiField = SyntaxEx.FieldDeclaration(
                new[] { SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword }, Names.WikiInfo, "m_wiki");

            var wikiParameter = SyntaxEx.Parameter(Names.WikiInfo, "wiki");

            var ctor = SyntaxEx.ConstructorDeclaration(
                new[] { SyntaxKind.InternalKeyword }, Names.QueryAction,
                new[] { wikiParameter },
                new StatementSyntax[] { SyntaxEx.Assignment(wikiField, wikiParameter) });

            var queryActionClass = SyntaxEx.ClassDeclaration(Names.QueryAction, wikiField, ctor);

            Files.Add(
                Names.QueryAction, SyntaxEx.CompilationUnit(
                    SyntaxEx.NamespaceDeclaration(Namespace, queryActionClass),
                    "System", "System.Collections.Generic", "LinqToWiki.Collections", "LinqToWiki.Parameters"));
        }

        private void CreateWikiClass()
        {
            var queryProperty = SyntaxEx.AutoPropertyDeclaration(
                new[] { SyntaxKind.PublicKeyword }, Names.QueryAction, "Query", SyntaxKind.PrivateKeyword);

            var baseUriParameter = SyntaxEx.Parameter("string", "baseUri");
            var apiPathParameter = SyntaxEx.Parameter("string", "apiPath");

            var queryAssignment = SyntaxEx.Assignment(
                queryProperty,
                SyntaxEx.ObjectCreation(
                    Names.QueryAction,
                    SyntaxEx.ObjectCreation(
                        Names.WikiInfo, (NamedNode)baseUriParameter, (NamedNode)apiPathParameter)));

            var ctorWithParameters = SyntaxEx.ConstructorDeclaration(
                new[] { SyntaxKind.PublicKeyword }, Names.Wiki,
                new[] { baseUriParameter, apiPathParameter },
                new StatementSyntax[] { queryAssignment });

            var ctorWithoutParameters = SyntaxEx.ConstructorDeclaration(
                new[] { SyntaxKind.PublicKeyword }, Names.Wiki, new ParameterSyntax[0], new StatementSyntax[0],
                SyntaxEx.ThisConstructorInitializer(SyntaxEx.NullLiteral(), SyntaxEx.NullLiteral()));

            var wikiClass = SyntaxEx.ClassDeclaration(
                Names.Wiki, queryProperty, ctorWithParameters, ctorWithoutParameters);

            Files.Add(Names.Wiki, SyntaxEx.CompilationUnit(SyntaxEx.NamespaceDeclaration(Namespace, wikiClass)));
        }

        private void CreateEnumsFile()
        {
            Files.Add(Names.Enums, SyntaxEx.CompilationUnit(SyntaxEx.NamespaceDeclaration(Namespace)));
        }

        private void RetrieveModuleNames()
        {
            var paramInfo = m_processor
                .Execute(QueryParameters.Create<ParamInfo>().AddSingleValue("modules", "paraminfo"))
                .Single();

            m_moduleNames = ((EnumParameterType)paramInfo.Parameters.Single(p => p.Name == "modules").Type).Values.ToArray();
            m_queryModuleNames = ((EnumParameterType)paramInfo.Parameters.Single(p => p.Name == "querymodules").Type).Values.ToArray();
        }

        public IEnumerable<string> GetAllQueryModuleNames()
        {
            if (m_queryModuleNames == null)
                RetrieveModuleNames();

            return m_queryModuleNames;
        }

        public IEnumerable<string> GetAllModuleNames()
        {
            if (m_moduleNames == null)
                RetrieveModuleNames();

            return m_moduleNames;
        }

        public IEnumerable<ParamInfo> GetQueryModules(IEnumerable<string> moduleNames)
        {
            return m_processor
                .Execute(QueryParameters.Create<ParamInfo>().AddMultipleValues("querymodules", moduleNames));
        }

        public IEnumerable<ParamInfo> GetModules(IEnumerable<string> moduleNames)
        {
            return m_processor
                .Execute(QueryParameters.Create<ParamInfo>().AddMultipleValues("modules", moduleNames));
        }

        public void AddQueryModule(string moduleName)
        {
            AddQueryModules(new[] { moduleName });
        }

        public void AddQueryModules(IEnumerable<string> moduleNames)
        {
            var paramInfos = GetQueryModules(moduleNames);

            foreach (var paramInfo in paramInfos.Take(1))
            {
                if (paramInfo.QueryType == QueryType.List)
                    AddListModule(paramInfo);

                // TODO: other types of modules
            }
        }

        public void AddAllQueryModules()
        {
            AddQueryModules(GetAllQueryModuleNames());
        }

        private void AddListModule(ParamInfo paramInfo)
        {
            new ListModuleGenerator(this).Generate(paramInfo);
        }

        public EmitResult Compile(string fileName)
        {
            var compilation = Compilation.Create(
                fileName, new CompilationOptions(assemblyKind: AssemblyKind.DynamicallyLinkedLibrary))
                .AddSyntaxTrees(Files.Select(f => SyntaxTree.Create(f.Item1 + Extension, f.Item2.Format())))
                .AddReferences(
                    new AssemblyFileReference(typeof(object).Assembly.Location),
                    new AssemblyFileReference(typeof(System.Xml.Linq.XElement).Assembly.Location),
                    new AssemblyFileReference(typeof(System.Xml.IXmlLineInfo).Assembly.Location),
                    new AssemblyFileReference(typeof(WikiInfo).Assembly.Location));

            return compilation.Emit(File.OpenWrite(fileName));
        }

        public void WriteToFiles(string directoryPath)
        {
            Directory.CreateDirectory(directoryPath);
            foreach (var file in Files)
            {
                var path = Path.Combine(directoryPath, file.Item1 + Extension);
                File.WriteAllText(path, file.Item2.Format().ToString());
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            bool first = true;

            foreach (var file in Files)
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
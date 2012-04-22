using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Collections;
using LinqToWiki.Parameters;
using Microsoft.CSharp;
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

        private int m_modulesFinished;

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
                    "paraminfo", "", null, null,
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
            var wikiField = SyntaxEx.FieldDeclaration(
                new[] { SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword }, Names.WikiInfo, "m_wiki");

            var queryProperty = SyntaxEx.AutoPropertyDeclaration(
                new[] { SyntaxKind.PublicKeyword }, Names.QueryAction, "Query", SyntaxKind.PrivateKeyword);

            var baseUriParameter = SyntaxEx.Parameter("string", "baseUri", SyntaxEx.NullLiteral());
            var apiPathParameter = SyntaxEx.Parameter("string", "apiPath", SyntaxEx.NullLiteral());

            var wikiAssignment = SyntaxEx.Assignment(
                wikiField,
                SyntaxEx.ObjectCreation(Names.WikiInfo, (NamedNode)baseUriParameter, (NamedNode)apiPathParameter));

            var queryAssignment = SyntaxEx.Assignment(
                queryProperty, SyntaxEx.ObjectCreation(Names.QueryAction, (NamedNode)wikiField));

            var ctor = SyntaxEx.ConstructorDeclaration(
                new[] { SyntaxKind.PublicKeyword }, Names.Wiki,
                new[] { baseUriParameter, apiPathParameter },
                new StatementSyntax[] { wikiAssignment, queryAssignment });

            var wikiClass = SyntaxEx.ClassDeclaration(Names.Wiki, wikiField, queryProperty, ctor);

            Files.Add(
                Names.Wiki,
                SyntaxEx.CompilationUnit(
                    SyntaxEx.NamespaceDeclaration(Namespace, wikiClass),
                    "LinqToWiki.Collections", "LinqToWiki.Parameters"));
        }

        private void CreateEnumsFile()
        {
            Files.Add(
                Names.Enums,
                SyntaxEx.CompilationUnit(
                    SyntaxEx.NamespaceDeclaration(Namespace), "System", "System.ComponentModel", "System.Globalization"));
        }

        private void RetrieveModuleNames()
        {
            var module = m_processor
                .ExecuteSingle(QueryParameters.Create<ParamInfo>().AddSingleValue("modules", "paraminfo"))
                .Modules.Single();

            m_moduleNames = ((EnumParameterType)module.Parameters.Single(p => p.Name == "modules").Type).Values.ToArray();
            m_queryModuleNames = ((EnumParameterType)module.Parameters.Single(p => p.Name == "querymodules").Type).Values.ToArray();
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

        public IEnumerable<Module> GetQueryModules(IEnumerable<string> moduleNames)
        {
            return m_processor
                .ExecuteSingle(QueryParameters.Create<ParamInfo>().AddMultipleValues("querymodules", moduleNames))
                .QueryModules;
        }

        public IEnumerable<Module> GetModules(IEnumerable<string> moduleNames)
        {
            return m_processor
                .ExecuteSingle(QueryParameters.Create<ParamInfo>().AddMultipleValues("modules", moduleNames))
                .Modules;
        }

        public void AddQueryModule(string moduleName)
        {
            AddQueryModules(new[] { moduleName });
        }

        public void AddQueryModules(IEnumerable<string> moduleNames)
        {
            var modules = GetQueryModules(moduleNames);

            foreach (var module in modules.Take(39))
            {
                if (module.QueryType == QueryType.List || module.QueryType == QueryType.Meta)
                    AddListModule(module);

                // TODO: other types of modules
            }
        }

        public void AddModules(IEnumerable<string> moduleNames)
        {
            var modules = GetModules(moduleNames);

            foreach (var module in modules.Where(p => p.Name == "login"))
                AddModule(module);
        }

        public void AddAllQueryModules()
        {
            AddQueryModules(GetAllQueryModuleNames());
        }

        public void AddAllModules()
        {
            AddModules(GetAllModuleNames());
        }

        private void AddListModule(Module module)
        {
            new ListModuleGenerator(this).Generate(module);
        }

        private void AddModule(Module module)
        {
            new ModuleGenerator(this).Generate(module);
        }

        public CompilerResults Compile(string name)
        {
            if (m_modulesFinished == 0)
                throw new InvalidOperationException("No modules were successfully finished, nothing to compile.");

            var compiler = new CSharpCodeProvider();

            return compiler.CompileAssemblyFromSource(
                new CompilerParameters(
                    new[]
                    {
                        typeof(System.ComponentModel.TypeConverter).Assembly.Location,
                        typeof(System.Xml.Linq.XElement).Assembly.Location,
                        typeof(System.Xml.IXmlLineInfo).Assembly.Location,
                        typeof(WikiInfo).Assembly.Location
                    }, name + ".dll")
                { TreatWarningsAsErrors = true, CompilerOptions = string.Format("/doc:{0}.xml /nowarn:1591", name) },
                Files.Select(f => f.Item2.Format().ToString()).ToArray());
        }

        public void WriteToFiles(string directoryPath)
        {
            if (m_modulesFinished == 0)
                throw new InvalidOperationException("No modules were successfully finished, nothing to write out.");

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

        public void ModuleFinished()
        {
            m_modulesFinished++;
        }
    }
}
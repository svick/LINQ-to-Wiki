using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LinqToWiki.Codegen.ModuleGenerators;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Collections;
using LinqToWiki.Parameters;
using Microsoft.CSharp;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen
{
    public class Wiki
    {
        // TODO: split processing parameters

        internal static class Names
        {
            public const string Wiki = "Wiki";
            public const string WikiInfo = "WikiInfo";
            public const string QueryAction = "QueryAction";
            public const string Enums = "Enums";
            public const string Page = "Page";
            public const string PageResult = "PageResult";
        }

        private readonly QueryProcessor<ParamInfo> m_processor;

        private const string Extension = ".cs";

        private string[] m_moduleNames;
        private string[] m_queryModuleNames;

        private int m_modulesFinished;

        internal string Namespace { get; private set; }
        internal string EntitiesNamespace { get; private set; }

        internal TupleList<string, CompilationUnitSyntax> Files { get; private set; }

        internal TypeManager TypeManager { get; private set; }

        internal static readonly TupleList<string, string> QueryBaseParameters =
            new TupleList<string, string> { { "action", "query" } };

        public Wiki(string baseUri, string apiPath, string ns = null)
        {
            Files = new TupleList<string, CompilationUnitSyntax>();
            TypeManager = new TypeManager(this);

            Namespace = ns ?? "LinqToWiki";
            EntitiesNamespace = Namespace + ".Entities";

            m_processor = new QueryProcessor<ParamInfo>(
                new WikiInfo(baseUri, apiPath),
                new QueryTypeProperties<ParamInfo>(
                    "paraminfo", "", null, null,
                    new TupleList<string, string> { { "action", "paraminfo" } },
                    null,
                    ParamInfo.Parse));

            CreatePageClass();
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
                    "System", "System.Collections.Generic", "LinqToWiki.Collections", "LinqToWiki.Parameters",
                    EntitiesNamespace));
        }

        private void CreatePageClass()
        {
            var pageClass = SyntaxEx.ClassDeclaration(Names.Page);

            Files.Add(
                Names.Page,
                SyntaxEx.CompilationUnit(
                    SyntaxEx.NamespaceDeclaration(Namespace, pageClass),
                    "System", "System.Collections.Generic", "LinqToWiki.Collections", EntitiesNamespace));
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

            var members = new List<MemberDeclarationSyntax> { wikiField, queryProperty, ctor };

            members.AddRange(CreatePageSourceMethods(wikiField));

            var wikiClass = SyntaxEx.ClassDeclaration(Names.Wiki, members);

            Files.Add(
                Names.Wiki,
                SyntaxEx.CompilationUnit(
                    SyntaxEx.NamespaceDeclaration(Namespace, wikiClass),
                    "System.Collections.Generic", "LinqToWiki.Collections", "LinqToWiki.Parameters", EntitiesNamespace));
        }

        private static IEnumerable<MethodDeclarationSyntax> CreatePageSourceMethods(FieldDeclarationSyntax wikiField)
        {
            var pageSources =
                new[]
                {
                    new { type = "string", name = "titles", sourceType = typeof(TitlesSource<>) },
                    new { type = "long", name = "pageIds", sourceType = typeof(PageIdsSource<>) },
                    new { type = "long", name = "revIds", sourceType = typeof(RevIdsSource<>) }
                };

            foreach (var pageSource in pageSources)
            {
                string sourceTypeName = pageSource.sourceType.Name;
                sourceTypeName = sourceTypeName.Substring(0, sourceTypeName.IndexOf('`'));

                var parameterVersions =
                    new[]
                    {
                        SyntaxEx.Parameter(SyntaxEx.GenericName("IEnumerable", pageSource.type), pageSource.name),
                        SyntaxEx.Parameter(
                            pageSource.type + "[]", pageSource.name, modifiers: new[] { SyntaxKind.ParamsKeyword })
                    };

                foreach (var parameter in parameterVersions)
                {
                    var returnStatement = SyntaxEx.Return(
                        SyntaxEx.ObjectCreation(
                            SyntaxEx.GenericName(sourceTypeName, Names.Page),
                            (NamedNode)wikiField, (NamedNode)parameter));

                    yield return SyntaxEx.MethodDeclaration(
                        new[] { SyntaxKind.PublicKeyword }, SyntaxEx.GenericName("PagesSource", Names.Page),
                        "Create" + sourceTypeName, new[] { parameter }, returnStatement);
                }
            }
        }

        private void CreateEnumsFile()
        {
            Files.Add(
                Names.Enums,
                SyntaxEx.CompilationUnit(
                    SyntaxEx.NamespaceDeclaration(Namespace), "System", "System.ComponentModel", "System.Globalization"));
        }

        private void CreatePageResultClass()
        {
            var infoResultClassName = Files["info"].SingleDescendant<ClassDeclarationSyntax>().Identifier.ValueText;
            var typeParameterName = "TData";
            var dataPropertyType = SyntaxEx.GenericName("IEnumerable", typeParameterName);

            var infoProperty = SyntaxEx.AutoPropertyDeclaration(
                new[] { SyntaxKind.PublicKeyword }, infoResultClassName, "Info", SyntaxKind.PrivateKeyword);

            var dataProperty = SyntaxEx.AutoPropertyDeclaration(
                new[] { SyntaxKind.PublicKeyword }, dataPropertyType, "Data", SyntaxKind.PrivateKeyword);

            var infoParameter = SyntaxEx.Parameter(infoResultClassName, "info");
            var dataParameter = SyntaxEx.Parameter(dataPropertyType, "data");

            var ctorBody =
                new StatementSyntax[]
                {
                    SyntaxEx.Assignment(infoProperty, infoParameter),
                    SyntaxEx.Assignment(dataProperty, dataParameter)
                };

            var ctor = SyntaxEx.ConstructorDeclaration(
                new[] { SyntaxKind.PublicKeyword }, Names.PageResult, new[] { infoParameter, dataParameter }, ctorBody);

            var pageResultClass = SyntaxEx.ClassDeclaration(
                Names.PageResult, new[] { SyntaxEx.TypeParameter(typeParameterName) }, null,
                new MemberDeclarationSyntax[] { infoProperty, dataProperty, ctor });

            var pageResultType = SyntaxEx.GenericName(Names.PageResult, typeParameterName);

            var createMethodBody = SyntaxEx.Return(
                SyntaxEx.ObjectCreation(pageResultType, (NamedNode)infoParameter, (NamedNode)dataParameter));

            var createMethod = SyntaxEx.MethodDeclaration(
                new[] { SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword }, pageResultType, "Create",
                new[] { SyntaxEx.TypeParameter(typeParameterName) }, new[] { infoParameter, dataParameter },
                createMethodBody);

            var pageResultHelperClass = SyntaxEx.ClassDeclaration(true, Names.PageResult, createMethod);

            Files.Add(
                Names.PageResult,
                SyntaxEx.CompilationUnit(
                    SyntaxEx.NamespaceDeclaration(Namespace, pageResultClass, pageResultHelperClass),
                    "System.Collections.Generic", EntitiesNamespace));
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

            int propModules = 0;

            foreach (var module in modules)
            {
                if (module.QueryType == QueryType.List || module.QueryType == QueryType.Meta)
                    AddListModule(module);
                else
                {
                    if (module.PropertyGroups == null)
                        continue;

                    if (module.Name == "info")
                    {
                        AddInfoModule(module);
                        CreatePageResultClass();
                    }
                    else if (module.PropertyGroups.Any(g => g.Name == null))
                    {
                        // TODO
                    }
                    else if (propModules++ < 9)
                    {
                        // TODO revisons
                        if (module.Name == "revisions")
                            continue;
                        Console.WriteLine("Adding module {0}.", module.Name);
                        AddPropModule(module);
                    }
                }
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

        private void AddPropModule(Module module)
        {
            new PropModuleGenerator(this).Generate(module);
        }

        private void AddInfoModule(Module module)
        {
            new InfoModuleGenerator(this).Generate(module);
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

            // TODO: use actual Temp directory
            var files = WriteToFiles(@"C:\Temp\generated");

            return compiler.CompileAssemblyFromFile(
                new CompilerParameters(
                    new[]
                    {
                        typeof(System.ComponentModel.TypeConverter).Assembly.Location,
                        typeof(System.Xml.Linq.XElement).Assembly.Location,
                        typeof(System.Xml.IXmlLineInfo).Assembly.Location,
                        typeof(WikiInfo).Assembly.Location
                    }, name + ".dll")
                {
                    TreatWarningsAsErrors = true,
                    CompilerOptions = string.Format("/doc:{0}.xml /nowarn:1591", name),
                    IncludeDebugInformation = true
                },
                files.ToArray());
        }

        public IEnumerable<string> WriteToFiles(string directoryPath)
        {
            if (m_modulesFinished == 0)
                throw new InvalidOperationException("No modules were successfully finished, nothing to write out.");

            var result = new List<string>();

            Directory.CreateDirectory(directoryPath);
            foreach (var file in Files)
            {
                var path = Path.Combine(directoryPath, file.Item1 + Extension);
                File.WriteAllText(path, file.Item2.Format().ToString());
                result.Add(path);
            }

            return result;
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
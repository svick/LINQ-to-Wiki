using System.Xml.Linq;
using Microsoft.Pex.Framework.Suppression;
using System.Linq.Expressions;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using LinqToWiki.Download;
using LinqToWiki.Internals;
using Microsoft.Pex.Framework.Using;
// <copyright file="PexAssemblyInfo.cs">Copyright ©  2011</copyright>
using Microsoft.Pex.Framework.Coverage;
using Microsoft.Pex.Framework.Creatable;
using Microsoft.Pex.Framework.Instrumentation;
using Microsoft.Pex.Framework.Settings;
using Microsoft.Pex.Framework.Validation;

// Microsoft.Pex.Framework.Settings
[assembly: PexAssemblySettings(TestFramework = "VisualStudioUnitTest")]

// Microsoft.Pex.Framework.Instrumentation
[assembly: PexAssemblyUnderTest("LinqToWiki.Core")]
[assembly: PexInstrumentAssembly("Microsoft.CSharp")]
[assembly: PexInstrumentAssembly("IQToolkit")]
[assembly: PexInstrumentAssembly("RestSharp")]
[assembly: PexInstrumentAssembly("System.Core")]
[assembly: PexInstrumentAssembly("System.Xml.Linq")]

// Microsoft.Pex.Framework.Creatable
[assembly: PexCreatableFactoryForDelegates]

// Microsoft.Pex.Framework.Validation
[assembly: PexAllowedContractRequiresFailureAtTypeUnderTestSurface]
[assembly: PexAllowedXmlDocumentedException]

// Microsoft.Pex.Framework.Coverage
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "Microsoft.CSharp")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "IQToolkit")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "RestSharp")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "System.Core")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "System.Xml.Linq")]
[assembly: PexUseType(typeof(HttpQueryParameter))]
[assembly: PexUseType(typeof(HttpQueryFileParameter))]
[assembly: PexUseType(typeof(BinaryExpression))]
[assembly: PexUseType(typeof(ConstantExpression))]
//[assembly: PexUseType(typeof(MemberExpression))]
//[assembly: PexUseType(typeof(MethodCallExpression))]
//[assembly: PexUseType(typeof(ParameterExpression))]
[assembly: PexUseType(typeof(UnaryExpression))]
[assembly: PexUseType(typeof(GC), "System.RuntimeType")]
[assembly: PexUseType(typeof(GC), "System.Reflection.RtFieldInfo")]
[assembly: PexUseType(typeof(GC), "System.Reflection.RuntimeMethodInfo")]
[assembly: PexUseType(typeof(GC), "System.Reflection.RuntimePropertyInfo")]
[assembly: PexUseType(typeof(GC), "System.Reflection.RuntimeConstructorInfo")]
[assembly: PexUseType(typeof(QueryTypeProperties<int>))]
[assembly: PexUseType(typeof(XText))]
[assembly: PexUseType(typeof(XCData))]
[assembly: PexUseType(typeof(XElement))]
[assembly: PexUseType(typeof(XComment))]
[assembly: PexUseType(typeof(XProcessingInstruction))]
[assembly: PexUseType(typeof(XAttribute))]
[assembly: PexUseType(typeof(XDocument))]
[assembly: PexUseType(typeof(Dictionary<string, string[]>))]


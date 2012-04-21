using System;
using System.Collections.Generic;
using System.Linq;
using LinqToWiki.Codegen.ModuleInfo;
using LinqToWiki.Collections;
using Roslyn.Compilers.CSharp;

namespace LinqToWiki.Codegen
{
    class TypeManager
    {
        private readonly Wiki m_wiki;

        private readonly Dictionary<EnumParameterType, string> m_enumTypes = new Dictionary<EnumParameterType, string>();

        public TypeManager(Wiki wiki)
        {
            m_wiki = wiki;
        }

        public string GetTypeName(Property property)
        {
            return GetTypeName(property.Type, property.Name);
        }

        public string GetTypeName(Parameter parameter)
        {
            return GetTypeName(parameter.Type, parameter.Name);
        }

        public string GetTypeName(ParameterType parameterType, string propertyName, bool nullable = false)
        {
            var simpleType = parameterType as SimpleParameterType;
            if (simpleType != null)
                return GetSimpleTypeName(simpleType, propertyName, nullable);

            return GetEnumTypeName((EnumParameterType)parameterType, propertyName, nullable);
        }

        private static string GetSimpleTypeName(SimpleParameterType simpleType, string propertyName, bool nullable)
        {
            string result;

            switch (simpleType.Name)
            {
            case "string":
            case "user":
                result = "string";
                break;
            case "timestamp":
                result = "DateTime";
                break;
            case "namespace":
                result = "Namespace";
                break;
            case "boolean":
                result = "bool";
                break;
            case "integer":
                result = propertyName.EndsWith("id") ? "long" : "int";
                break;
            default:
                throw new InvalidOperationException(string.Format("Unknown type {0}", simpleType.Name));
            }

            if (nullable && simpleType.Name != "string" && simpleType.Name != "namespace")
                result += '?';

            return result;
        }

        private string GetEnumTypeName(EnumParameterType enumType, string propertyName, bool nullable)
        {
            // TODO: better type naming (use module name)
            if (!m_enumTypes.ContainsKey(enumType))
                GenerateType(enumType, propertyName);

            string result = m_enumTypes[enumType];

            if (nullable)
                result += '?';

            return result;
        }

        private void GenerateType(EnumParameterType enumType, string propertyName)
        {
            string typeName = propertyName;
            int i = 1;
            while (m_enumTypes.Values.Contains(typeName))
                typeName = propertyName + i;

            var enumsFile = m_wiki.Files[Wiki.Names.Enums];

            var namespaceDeclaration = enumsFile.SingleDescendant<NamespaceDeclarationSyntax>();

            var fixedMemberNameMapping = new TupleList<string, string>();
            var memberNames = new List<string>();

            foreach (var name in enumType.Values)
            {
                var fixedName = FixEnumMemberName(name);

                if (name != fixedName)
                    fixedMemberNameMapping.Add(fixedName, name);

                memberNames.Add(fixedName);
            }

            var enumDeclaration = SyntaxEx.EnumDeclaration(typeName, memberNames);

            var newNamespaceDeclaration = namespaceDeclaration;

            if (fixedMemberNameMapping.Any())
            {
                var converter = GenerateConverter(typeName, fixedMemberNameMapping);

                newNamespaceDeclaration = newNamespaceDeclaration.WithAdditionalMembers(converter);

                enumDeclaration = enumDeclaration.WithAttribute(
                    SyntaxEx.AttributeDeclaration(
                        "TypeConverter", SyntaxEx.AttributeArgument(SyntaxEx.TypeOf(converter.Identifier.ValueText))));
            }

            newNamespaceDeclaration = newNamespaceDeclaration.WithAdditionalMembers(enumDeclaration);

            m_wiki.Files[Wiki.Names.Enums] = enumsFile.ReplaceNode(
                namespaceDeclaration, newNamespaceDeclaration);

            m_enumTypes.Add(enumType, typeName);
        }

        private static ClassDeclarationSyntax GenerateConverter(string enumName, IEnumerable<Tuple<string, string>> mapping)
        {
            var converterClassName = enumName + "Converter";

            var ctor = SyntaxEx.ConstructorDeclaration(
                new[] { SyntaxKind.PublicKeyword }, converterClassName, new ParameterSyntax[0], new StatementSyntax[0],
                SyntaxEx.BaseConstructorInitializer(SyntaxEx.TypeOf(enumName)));

            var contextParameter = SyntaxEx.Parameter("ITypeDescriptorContext", "context");
            var cultureParameter = SyntaxEx.Parameter("CultureInfo", "culture");
            var valueParameter = SyntaxEx.Parameter("object", "value");
            var destinationTypeParameter = SyntaxEx.Parameter("Type", "destinationType");

            var castedValueLocal = SyntaxEx.LocalDeclaration(
                enumName, "castedValue", SyntaxEx.Cast(enumName, (NamedNode)valueParameter));

            var switchStatement = SyntaxEx.Switch(
                (NamedNode)castedValueLocal,
                mapping.Select(
                    t =>
                    SyntaxEx.SwitchCase(
                        SyntaxEx.MemberAccess(enumName, t.Item1), SyntaxEx.Return(SyntaxEx.Literal(t.Item2)))));

            var condition = SyntaxEx.If(
                SyntaxEx.Equals((NamedNode)destinationTypeParameter, SyntaxEx.TypeOf("string")), switchStatement);

            var baseCall = SyntaxEx.Return(
                SyntaxEx.Invocation(
                    SyntaxEx.MemberAccess("base", "ConvertTo"),
                    (NamedNode)contextParameter, (NamedNode)cultureParameter,
                    (NamedNode)valueParameter, (NamedNode)destinationTypeParameter));

            var convertToMethod =
                SyntaxEx.MethodDeclaration(
                    new[] { SyntaxKind.PublicKeyword, SyntaxKind.OverrideKeyword }, "object", "ConvertTo",
                    new[] { contextParameter, cultureParameter, valueParameter, destinationTypeParameter }, 
                    castedValueLocal, condition, baseCall);

            var classDeclaration = SyntaxEx.ClassDeclaration(
                converterClassName, SyntaxEx.ParseTypeName("EnumConverter"), ctor, convertToMethod);

            return classDeclaration;
        }

        private static string FixEnumMemberName(string value)
        {
            if (value == string.Empty)
                return "none";

            if (value[0] == '!')
                value = "not-" + value.Substring(1);

            return value.Replace('-', '_').Replace('/', '_');
        }

        // value is expected to be a string
        public ExpressionSyntax CreateConverter(Property property, ExpressionSyntax value, ExpressionSyntax wiki)
        {
            var simpleType = property.Type as SimpleParameterType;
            if (simpleType != null)
                return CreateSimpleConverter(simpleType, property.Name, value, wiki);

            return CreateEnumConverter((EnumParameterType)property.Type, value);
        }

        private static ExpressionSyntax CreateSimpleConverter(
            SimpleParameterType simpleType, string propertyName, ExpressionSyntax value, ExpressionSyntax wiki)
        {
            if (simpleType.Name == "namespace")
                return SyntaxEx.Invocation(SyntaxEx.MemberAccess("ValueParser", "ParseNamespace"), value, wiki);

            string typeName;

            switch (simpleType.Name)
            {
            case "string":
            case "user":
                typeName = "String";
                break;
            case "timestamp":
                typeName = "DateTime";
                break;
            case "boolean":
                typeName = "Boolean";
                break;
            case "integer":
                typeName = propertyName.EndsWith("id") ? "Int64" : "Int32";
                break;
            default:
                throw new InvalidOperationException(string.Format("Unknown type {0}", simpleType.Name));
            }

            return SyntaxEx.Invocation(SyntaxEx.MemberAccess("ValueParser", "Parse" + typeName), value);
        }

        private ExpressionSyntax CreateEnumConverter(EnumParameterType type, ExpressionSyntax value)
        {
            var typeName = m_enumTypes[type];
            return SyntaxEx.Cast(
                typeName, SyntaxEx.Invocation(SyntaxEx.MemberAccess("Enum", "Parse"), SyntaxEx.TypeOf(typeName), value));
        }
    }
}
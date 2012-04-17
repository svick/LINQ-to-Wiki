using System;
using System.Collections.Generic;
using LinqToWiki.Codegen.ModuleInfo;
using System.Linq;
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

            var enumDeclaration = SyntaxEx.EnumDeclaration(typeName, enumType.Values.Select(FixEnumMemberName));

            m_wiki.Files[Wiki.Names.Enums] = enumsFile.ReplaceNode(
                namespaceDeclaration, namespaceDeclaration.WithAdditionalMembers(enumDeclaration));

            m_enumTypes.Add(enumType, typeName);
        }

        private static string FixEnumMemberName(string value)
        {
            if (value == string.Empty)
                return "none";

            if (value[0] == '!')
                value = "not-" + value.Substring(1);

            return value.Replace('-', '_');
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
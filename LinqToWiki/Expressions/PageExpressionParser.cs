using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;
using LinqToWiki.Parameters;

namespace LinqToWiki.Expressions
{
    public static class PageExpressionParser
    {
        public static PageQueryParameters ParseSelect<TSource, TResult>(
            Expression<Func<TSource, TResult>> expression, PageQueryParameters baseParameters,
            out Func<PageData, TResult> processedExpression)
        {
            var parameters = PageSelectVisitor.Process(expression, out processedExpression);
            return baseParameters.WithParameters(parameters);
        }
    }

    class PageSelectVisitor : ExpressionVisitor
    {
        /// <summary>
        /// Result properties that are always present and don't need prop=info
        /// </summary>
        private static readonly IEnumerable<string> NonInfoProperties = new[] { "pageid", "ns", "title" };

        private static readonly MethodInfo PageDataGetInfoMethod = typeof(PageData).GetMethod("GetInfo");
        private static readonly MethodInfo PageDataGetDataMethod = typeof(PageData).GetMethod("GetData");

        private readonly ParameterExpression m_pageParameter;
        private readonly ParameterExpression m_pageDataParameter = Expression.Parameter(typeof(PageData), "pageData");
        private readonly MethodCallExpression m_pageDataGetInfoCall;
        private readonly Type m_infoType;

        private readonly Dictionary<string, PropQueryParameters> m_parameters = new Dictionary<string, PropQueryParameters>();

        private PageSelectVisitor(ParameterExpression pageParameter)
        {
            m_pageParameter = pageParameter;

            m_infoType = pageParameter.Type.GetProperty("info").PropertyType;

            m_pageDataGetInfoCall = Expression.Call(
                m_pageDataParameter, PageDataGetInfoMethod.MakeGenericMethod(m_infoType));
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression == m_pageParameter && node.Member.Name == "info")
                return m_pageDataGetInfoCall;

            return base.VisitMember(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var methodName = node.Method.Name;
            var declaringType = node.Method.DeclaringType;

            if (node.Object == m_pageParameter)
            {
                if (declaringType == typeof(object))
                    throw new InvalidOperationException(
                        string.Format("The method '{0}' is not supported.", methodName));

                return Expression.Call(
                    m_pageDataParameter,
                    PageDataGetDataMethod.MakeGenericMethod(node.Method.ReturnType.GetGenericArguments().Last()),
                    Expression.Constant(methodName));
            }

            if (declaringType.IsGenericType)
            {
                if (declaringType.GetGenericTypeDefinition() == typeof(WikiQueryResult<,>))
                {
                    var propExpression = ExpressionFinder.Single<MethodCallExpression>(
                        node.Object, e => e.Object == m_pageParameter && e.Method.DeclaringType == m_pageParameter.Type);

                    var createQueryParametersMethod = typeof(QueryParameters).GetMethod("Create")
                        .MakeGenericMethod(propExpression.Type.GetGenericArguments().Last());

                    var queryObject = Activator.CreateInstance(
                        propExpression.Type, new[] { null, createQueryParametersMethod.Invoke(null, null) });

                    var withQueryObject = ExpressionReplacer.Replace(
                        node.Object, propExpression, Expression.Constant(queryObject));

                    var processedQueryObject =
                        Expression.Lambda<Func<IWikiQueryResult>>(withQueryObject).Compile().Invoke();

                    var parameter = new PropQueryParameters(propExpression.Method.Name);

                    parameter.CopyFrom(processedQueryObject.Parameters);

                    m_parameters.Add(propExpression.Method.Name, parameter);

                    var obj = Visit(node.Object);

                    if (methodName == "ToEnumerable")
                        return obj;

                    return Expression.Call(
                        typeof(Enumerable), methodName, new[] { obj.Type.GetGenericArguments().Single() }, obj);
                }
                if (BaseTypes(declaringType).Contains(typeof(WikiQueryResult<,>)))
                {
                    var obj = Visit(node.Object);

                    if (methodName != "Select")
                        return obj;

                    var argument = ((UnaryExpression)node.Arguments.Single()).Operand;
                    var genericArguments = argument.Type.GetGenericArguments();

                    return Expression.Call(
                        typeof(Enumerable), methodName, new[] { genericArguments[0], genericArguments[1] }, obj, argument);
                }
            }

            return base.VisitMethodCall(node);
        }

        private static IEnumerable<Type> BaseTypes(Type type)
        {
            while (true)
            {
                type = type.BaseType;

                if (type == null)
                    yield break;

                yield return type.GetGenericTypeDefinition();
            }
        }

        public static IEnumerable<PropQueryParameters> Process<TSource, TResult>(
            Expression<Func<TSource, TResult>> expression, out Func<PageData, TResult> processedExpression)
        {
            var visitor = new PageSelectVisitor(expression.Parameters.Single());
            var body = visitor.Visit(expression.Body);

            var gatherer = new UsedPropertiesGatherer();
            gatherer.Gather(body, visitor.m_pageDataGetInfoCall);

            var parameters = visitor.m_parameters;

            // TODO: handle tokens in a special way

            if (gatherer.UsedDirectly)
                parameters.Add("info", new PropQueryParameters("info"));
            else if (gatherer.UsedProperties.Any(p => !NonInfoProperties.Contains(p)))
                parameters.Add("info", new PropQueryParameters("info").WithProperties(gatherer.UsedProperties));

            processedExpression =
                Expression.Lambda<Func<PageData, TResult>>(body, visitor.m_pageDataParameter).Compile();

            return parameters.Values;
        }
    }
}
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
        private readonly ParameterExpression m_pageParameter;
        private readonly ParameterExpression m_pageDataParameter = Expression.Parameter(typeof(PageData), "pageData");
        private readonly MethodCallExpression m_pageDataGetInfoCall;
        private readonly Type m_infoType;

        private readonly Dictionary<string, PropQueryParameters> m_parameters = new Dictionary<string, PropQueryParameters>();

        private PageSelectVisitor(ParameterExpression pageParameter)
        {
            m_pageParameter = pageParameter;

            m_infoType = pageParameter.Type.GetProperty("info").PropertyType;

            var parseDelegate = Delegate.CreateDelegate(
                typeof(Func<,,>).MakeGenericType(typeof(XElement), typeof(WikiInfo), m_infoType),
                m_infoType.GetMethod("Parse"));

            m_pageDataGetInfoCall = Expression.Call(
                m_pageDataParameter, PageDataGetInfoMethod.MakeGenericMethod(m_infoType),
                Expression.Constant(parseDelegate));
        }

        private static readonly MethodInfo PageDataGetInfoMethod = typeof(PageData).GetMethod("GetInfo");

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression == m_pageParameter && node.Member.Name == "info")
                return m_pageDataGetInfoCall;

            return base.VisitMember(node);
        }

        public static IEnumerable<PropQueryParameters> Process<TSource, TResult>(
            Expression<Func<TSource, TResult>> expression, out Func<PageData, TResult> processedExpression)
        {
            var visitor = new PageSelectVisitor(expression.Parameters.Single());
            var body = visitor.Visit(expression.Body);

            var gatherer = new UsedPropertiesGatherer();
            gatherer.Gather(body, visitor.m_pageDataGetInfoCall);

            var parameters = visitor.m_parameters;

            if (gatherer.UsedDirectly)
                parameters.Add("info", new PropQueryParameters("info", typeof(TSource)));
            else if (gatherer.UsedProperties.Any())
                parameters.Add("info", new PropQueryParameters("info", typeof(TSource)).WithProperties(gatherer.UsedProperties));

            processedExpression =
                Expression.Lambda<Func<PageData, TResult>>(body, visitor.m_pageDataParameter).Compile();

            return parameters.Values;
        }
    }
}
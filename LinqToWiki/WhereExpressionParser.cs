using System;
using System.Linq.Expressions;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    static class WhereExpressionParser
    {
        public static QueryParameter Parse(LambdaExpression expression, QueryParameter previousParameters)
        {
            // TODO: parse more complicated expressions

            var body = expression.Body;

            var binaryExpression = body as BinaryExpression;

            if (binaryExpression != null)
            {
                if (binaryExpression.NodeType == ExpressionType.Equal)
                {
                    return ParseEqualExpression(binaryExpression, previousParameters);
                }
            }

            throw new ArgumentException();
        }

        private static QueryParameter ParseEqualExpression(BinaryExpression expression, QueryParameter previousParameters)
        {
            // TODO: handle reverse order

            var memberAccess = expression.Left as MemberExpression;

            if (memberAccess == null)
                throw new ArgumentException();

            if (!(memberAccess.Expression is ParameterExpression))
                throw new ArgumentException();

            string property = memberAccess.Member.Name.ToLowerInvariant();

            // TODO!!! get value

            return null;
        }
    }
}
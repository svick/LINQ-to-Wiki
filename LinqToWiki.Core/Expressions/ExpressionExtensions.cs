using System.Linq.Expressions;

namespace LinqToWiki.Expressions
{
    /// <summary>
    /// Contains extension methods for <see cref="Expression"/> types.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Switches sides of a binary expression.
        /// </summary>
        public static BinaryExpression Switch(this BinaryExpression expression)
        {
            if (expression == null)
                return null;

            return Expression.MakeBinary(expression.NodeType, expression.Right, expression.Left);
        }
    }
}
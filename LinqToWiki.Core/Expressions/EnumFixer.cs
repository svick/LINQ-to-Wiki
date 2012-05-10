using System;
using System.Linq.Expressions;

namespace LinqToWiki.Expressions
{
    /// <summary>
    /// Fixes expression containing comparison between enum property and integer constant
    /// into one that uses enum value. This is necessary for further processing.
    /// </summary>
    /// <remarks>
    /// Expressions that compare enum property with a enum constant (e.g. <c>x.Expiry == Expiry.Indefinite</c>)
    /// are represented using a cast and an integer constant in compiler generated Expression
    /// (e.g. <c>(int)x.Expiry == 0</c>).
    /// 
    /// This class converts the latter form back into the former, so that it can be easily processesed later.
    /// </remarks>
    class EnumFixer : ExpressionVisitor
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            return PerformFix(node) ?? PerformFix(node.Switch()).Switch() ?? base.VisitBinary(node);
        }

        /// <summary>
        /// Actually modifies the expression.
        /// Works only on expressions that have the constant on the right side.
        /// </summary>
        private static BinaryExpression PerformFix(BinaryExpression node)
        {
            // doesn't work correctly for other types of operations (and it's not necessary)
            if (node.NodeType != ExpressionType.Equal && node.NodeType != ExpressionType.NotEqual)
                return null;

            var leftUnary = node.Left as UnaryExpression;
            var rightConstant = node.Right as ConstantExpression;

            if (leftUnary != null && leftUnary.NodeType == ExpressionType.Convert && leftUnary.Operand.Type.IsEnum
                && rightConstant != null)
            {
                var enumType = leftUnary.Operand.Type;
                var enumValue = Enum.ToObject(enumType, rightConstant.Value);

                return Expression.MakeBinary(node.NodeType, leftUnary.Operand, Expression.Constant(enumValue));
            }

            return null;
        }

        /// <summary>
        /// Fixes expression containing enum into better form.
        /// </summary>
        public static Expression Fix(Expression expression)
        {
            return new EnumFixer().Visit(expression);
        }
    }
}
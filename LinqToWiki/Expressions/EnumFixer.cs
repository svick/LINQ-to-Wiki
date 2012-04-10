using System;
using System.Linq.Expressions;

namespace LinqToWiki.Expressions
{
    public class EnumFixer : ExpressionVisitor
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var leftUnary = node.Left as UnaryExpression;
            var rightConstant = node.Right as ConstantExpression;

            if (leftUnary != null && leftUnary.NodeType == ExpressionType.Convert && leftUnary.Operand.Type.IsEnum
                && rightConstant != null)
            {
                var enumType = leftUnary.Operand.Type;
                var enumValue = Enum.ToObject(enumType, rightConstant.Value);

                return Expression.MakeBinary(node.NodeType, leftUnary.Operand, Expression.Constant(enumValue));
            }

            return base.VisitBinary(node);
        }

        public static Expression Fix(Expression expression)
        {
            return new EnumFixer().Visit(expression);
        }
    }
}
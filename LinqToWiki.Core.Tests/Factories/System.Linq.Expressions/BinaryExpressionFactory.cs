using Microsoft.Pex.Framework;

namespace System.Linq.Expressions
{
    /// <summary>A factory for System.Linq.Expressions.BinaryExpression instances</summary>
    public static partial class BinaryExpressionFactory
    {
        /// <summary>A factory for System.Linq.Expressions.BinaryExpression instances</summary>
        [PexFactoryMethod(typeof(BinaryExpression))]
        public static BinaryExpression Create(ExpressionType binaryType, Expression left, Expression right)
        {
            PexAssume.IsNotNull(left);
            PexAssume.IsNotNull(right);

            return Expression.MakeBinary(binaryType, left, right);
        }
    }
}

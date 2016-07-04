using Microsoft.Pex.Framework;

namespace System.Linq.Expressions
{
    /// <summary>A factory for System.Linq.Expressions.ConstantExpression instances</summary>
    public static partial class ConstantExpressionFactory
    {
        /// <summary>A factory for System.Linq.Expressions.ConstantExpression instances</summary>
        [PexFactoryMethod(typeof(ConstantExpression))]
        public static object Create(object value, Type type)
        {
            return Expression.Constant(value, type);
        }

        /// <summary>A factory for System.Linq.Expressions.ConstantExpression instances</summary>
        [PexFactoryMethod(typeof(ConstantExpression))]
        public static object Create(object value)
        {
            return Expression.Constant(value);
        }
    }
}

using Microsoft.Pex.Framework;

namespace System.Linq.Expressions
{
    /// <summary>A factory for System.Linq.Expressions.ParameterExpression instances</summary>
    public static partial class ParameterExpressionFactory
    {
        /// <summary>A factory for System.Linq.Expressions.ParameterExpression instances</summary>
        [PexFactoryMethod(typeof(ParameterExpression))]
        public static object Create(Type type, string name)
        {
            PexAssume.IsNotNull(type);

            return Expression.Parameter(type, name);
        }
    }
}

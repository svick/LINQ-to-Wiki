using System.Reflection;
using Microsoft.Pex.Framework;

namespace System.Linq.Expressions
{
    /// <summary>A factory for System.Linq.Expressions.MemberExpression instances</summary>
    public static partial class MemberExpressionFactory
    {
        /// <summary>A factory for System.Linq.Expressions.MemberExpression instances</summary>
        [PexFactoryMethod(typeof(MemberExpression))]
        public static object Create(Expression expression, MemberInfo member)
        {
            PexAssume.IsNotNull(member);

            return Expression.MakeMemberAccess(expression, member);
        }
    }
}

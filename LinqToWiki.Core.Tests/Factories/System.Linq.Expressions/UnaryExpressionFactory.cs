using System.Linq.Expressions;
// <copyright file="UnaryExpressionFactory.cs">Copyright ©  2011</copyright>

using System;
using Microsoft.Pex.Framework;

namespace System.Linq.Expressions
{
    /// <summary>A factory for System.Linq.Expressions.UnaryExpression instances</summary>
    public static partial class UnaryExpressionFactory
    {
        /// <summary>A factory for System.Linq.Expressions.UnaryExpression instances</summary>
        [PexFactoryMethod(typeof(UnaryExpression))]
        public static object Create(ExpressionType unaryType, Expression operand, Type type)
        {
            PexAssume.IsNotNull(operand);

            return Expression.MakeUnary(unaryType, operand, type);
        }
    }
}

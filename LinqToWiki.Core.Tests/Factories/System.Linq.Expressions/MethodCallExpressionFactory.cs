using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Pex.Framework;

namespace System.Linq.Expressions
{
    /// <summary>A factory for System.Linq.Expressions.MethodCallExpression instances</summary>
    public static partial class MethodCallExpressionFactory
    {
        /// <summary>A factory for System.Linq.Expressions.MethodCallExpression instances</summary>
        [PexFactoryMethod(typeof(MethodCallExpression))]
        public static MethodCallExpression Create(Expression instance, MethodInfo method, IEnumerable<Expression> arguments)
        {
            return Expression.Call(instance, method, arguments);
        }
    }
}

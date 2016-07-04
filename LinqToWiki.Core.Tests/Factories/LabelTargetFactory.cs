using Microsoft.Pex.Framework;

namespace System.Linq.Expressions
{
    /// <summary>A factory for System.Linq.Expressions.LabelTarget instances</summary>
    public static partial class LabelTargetFactory
    {
        /// <summary>A factory for System.Linq.Expressions.LabelTarget instances</summary>
        [PexFactoryMethod(typeof(LabelTarget))]
        public static object Create(Type type, string name)
        {
            return Expression.Label(type, name);
        }
    }
}

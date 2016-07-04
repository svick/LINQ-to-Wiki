using Microsoft.Pex.Framework;
using static System.Reflection.BindingFlags;

namespace System.Reflection
{
    /// <summary>A factory for System.RuntimeMethodInfoFactory instances</summary>
    public static partial class RuntimeMethodInfoFactory
    {
        /// <summary>A factory for System.RuntimeMethodInfo instances</summary>
        [PexFactoryMethod(typeof(GC), "System.Reflection.RuntimeMethodInfo")]
        public static MethodInfo Create(Type type, int index)
        {
            return type.GetMethods(Public | NonPublic | Static | Instance)[index];
        }
    }
}

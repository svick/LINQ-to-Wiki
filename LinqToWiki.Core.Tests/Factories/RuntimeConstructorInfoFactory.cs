using Microsoft.Pex.Framework;
using static System.Reflection.BindingFlags;

namespace System.Reflection
{
    /// <summary>A factory for System.RuntimeConstructorInfoFactory instances</summary>
    public static partial class RuntimeConstructorInfoFactory
    {
        /// <summary>A factory for System.RuntimeConstructorInfo instances</summary>
        [PexFactoryMethod(typeof(GC), "System.Reflection.RuntimeConstructorInfo")]
        public static ConstructorInfo Create(Type type, int index)
        {
            return type.GetConstructors(Public | NonPublic | Static | Instance)[index];
        }
    }
}

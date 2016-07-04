using Microsoft.Pex.Framework;
using static System.Reflection.BindingFlags;

namespace System.Reflection
{
    /// <summary>A factory for System.RuntimePropertyInfoFactory instances</summary>
    public static partial class RuntimePropertyInfoFactory
    {
        /// <summary>A factory for System.RuntimePropertyInfo instances</summary>
        [PexFactoryMethod(typeof(GC), "System.Reflection.RuntimePropertyInfo")]
        public static PropertyInfo Create(Type type, int index)
        {
            PexAssume.IsNotNull(type);

            return type.GetProperties(Public | NonPublic | Static | Instance)[index];
        }
    }
}

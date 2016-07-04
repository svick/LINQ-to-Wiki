using Microsoft.Pex.Framework;
using static System.Reflection.BindingFlags;

namespace System.Reflection
{
    /// <summary>A factory for System.RuntimeFieldInfoFactory instances</summary>
    public static partial class RuntimeFieldInfoFactory
    {
        /// <summary>A factory for System.RuntimeFieldInfo instances</summary>
        [PexFactoryMethod(typeof(GC), "System.Reflection.RtFieldInfo")]
        public static FieldInfo Create(Type type, int index)
        {
            return type.GetFields(Public | NonPublic | Static | Instance)[index];
        }
    }
}

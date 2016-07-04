using System.Collections.Generic;
using Microsoft.Pex.Framework;

namespace System
{
    /// <summary>A factory for System.RuntimeType instances</summary>
    public static partial class RuntimeTypeFactory
    {
        //// http://stackoverflow.com/a/35702989/41071
        ///// <summary>
        ///// Table that maps TypeCode to it's corresponding Type.
        ///// </summary>
        //private static readonly Dictionary<TypeCode, Type> TypeCodeToTypeMap = new Dictionary<TypeCode, Type>
        //{
        //    { TypeCode.Boolean, typeof(bool) },
        //    { TypeCode.Byte, typeof(byte) },
        //    { TypeCode.Char, typeof(char) },
        //    { TypeCode.DateTime, typeof(DateTime) },
        //    { TypeCode.DBNull, typeof(DBNull) },
        //    { TypeCode.Decimal, typeof(decimal) },
        //    { TypeCode.Double, typeof(double) },
        //    { TypeCode.Empty, null },
        //    { TypeCode.Int16, typeof(short) },
        //    { TypeCode.Int32, typeof(int) },
        //    { TypeCode.Int64, typeof(long) },
        //    { TypeCode.Object, typeof(object) },
        //    { TypeCode.SByte, typeof(sbyte) },
        //    { TypeCode.Single, typeof(Single) },
        //    { TypeCode.String, typeof(string) },
        //    { TypeCode.UInt16, typeof(UInt16) },
        //    { TypeCode.UInt32, typeof(UInt32) },
        //    { TypeCode.UInt64, typeof(UInt64) }
        //};

        ///// <summary>A factory for System.RuntimeType instances</summary>
        //[PexFactoryMethod(typeof(GC), "System.RuntimeType")]
        //public static Type Create(TypeCode typeCode)
        //{
        //    return TypeCodeToTypeMap[typeCode];
        //}

        /// <summary>A factory for System.RuntimeType instances</summary>
        [PexFactoryMethod(typeof(GC), "System.RuntimeType")]
        public static Type Create(int assemblyIndex, int typeIndex)
        {
            return AppDomain.CurrentDomain.GetAssemblies()[assemblyIndex].GetTypes()[typeIndex];
        }
    }
}

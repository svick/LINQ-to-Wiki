using System;
using System.Collections.Generic;

namespace LinqToWiki.Collections
{
    /// <summary>
    /// Class that can be compared with single item or a collection.
    /// 
    /// This class is “virtual”, it is used only in queries and will never be instantiated.
    /// </summary>
    /// <remarks>
    /// This class is used in queries that accept multiple values for some properties,
    /// but it also allows simple comparison with a single item without creating a collection.
    /// 
    /// Consider those two cases:
    /// <list type="bullet">
    /// <item><c>where x.prop == new[] { "foo", "bar" }</c></item>
    /// <item><c>where x.prop == "foo"</c></item>
    /// </list>
    /// 
    /// If the where-property was <c>IEnumerable&lt;string&gt;</c>, the first case would compile,
    /// but the second one wouldn't. If it was just <c>string</c>, the first case wouldn't compile.
    /// Using this class, both cases work.
    /// 
    /// This class isn't actually necessary, the user could always create a single-item collection.
    /// But this way there is a simpler syntax for the common case of single item.
    /// </remarks>
    public class ItemOrCollection<T>
    {
        private ItemOrCollection()
        {}

        public static bool operator ==(ItemOrCollection<T> first, T second)
        {
            throw new NotSupportedException();
        }

        public static bool operator !=(ItemOrCollection<T> first, T second)
        {
            throw new NotSupportedException();
        }

        public static bool operator ==(ItemOrCollection<T> first, IEnumerable<T> second)
        {
            throw new NotSupportedException();
        }

        public static bool operator !=(ItemOrCollection<T> first, IEnumerable<T> second)
        {
            throw new NotSupportedException();
        }

        #region To avoid compiler warning

        public override bool Equals(object obj)
        {
            throw new NotSupportedException();
        }

        public override int GetHashCode()
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
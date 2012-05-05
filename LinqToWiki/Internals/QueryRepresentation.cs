using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LinqToWiki.Internals
{
    public static class QueryRepresentation
    {
        public static string ToQueryString(this Namespace ns)
        {
            return ns.GetQueryRepresentation();
        }

        public static string ToQueryString(this string s)
        {
            return s;
        }

        public static string ToQueryString(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("o");
        }

        public static string ToQueryString(this bool b)
        {
            return b ? "" : null;
        }

        public static string ToQueryString(this int i)
        {
            return i.ToString(CultureInfo.InvariantCulture);
        }

        public static string ToQueryString(this long l)
        {
            return l.ToString(CultureInfo.InvariantCulture);
        }

        public static string ToQueryString(this StringValue stringValue)
        {
            return stringValue.ToString();
        }

        public static string ToQueryString<T>(this IEnumerable<T> collection)
        {
            return string.Join("|", collection.Select(x => ToQueryStringDynamic(x)));
        }

        private static string ToQueryString(object obj)
        {
            throw new InvalidOperationException(string.Format("Object '{0}' cannot converted to query string.", obj));
        }

        public static string ToQueryStringDynamic(object obj)
        {
            return ToQueryString((dynamic)obj);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;

namespace LinqToWiki.Internals
{
    /// <summary>
    /// Comverts types used in queries to their representation in the query as a string.
    /// </summary>
    public static class QueryRepresentation
    {
        /// <summary>
        /// Converts namespace object to its query representation: its id.
        /// </summary>
        public static string ToQueryString(this Namespace ns)
        {
            Contract.Requires(ns != null);

            return ns.Id.ToQueryString();
        }

        /// <summary>
        /// Converts a string to its query representation: itself.
        /// </summary>
        public static string ToQueryString(this string s)
        {
            return s;
        }

        /// <summary>
        /// Converts a DateTime to its query representation: ISO 8601 formatted string.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToQueryString(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("o");
        }

        /// <summary>
        /// Converts a bool to its query representation:
        /// an empty string for <c>true</c>, nothing (represented here as <c>null</c>) for <c>false</c>.
        /// </summary>
        public static string ToQueryString(this bool b)
        {
            return b ? string.Empty : null;
        }

        /// <summary>
        /// Converts an int to its query representation.
        /// </summary>
        public static string ToQueryString(this int i)
        {
            return i.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a long to its query representation.
        /// </summary>
        public static string ToQueryString(this long l)
        {
            return l.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a <see cref="StringValue"/> to its query representation: the underlying string value.
        /// </summary>
        public static string ToQueryString(this StringValue stringValue)
        {
            Contract.Requires(stringValue != null);

            return stringValue.ToString();
        }

        /// <summary>
        /// Converts a collection to its query representation:
        /// the query representations of each of the members, separated by <c>|</c>.
        /// </summary>
        public static string ToQueryString<T>(this IEnumerable<T> collection)
        {
            Contract.Requires(collection != null);

            return string.Join("|", collection.Select(x => ToQueryStringDynamic(x)));
        }

        /// <summary>
        /// “Tries” to convert an object of an unknown type to its query representation.
        /// Always throws <see cref="InvalidOperationException"/>.
        /// 
        /// Used to provide better exception on failure for <see cref="ToQueryStringDynamic"/>.
        /// </summary>
        private static string ToQueryString(object obj)
        {
            throw new InvalidOperationException(string.Format("Object '{0}' cannot converted to query string.", obj));
        }

        /// <summary>
        /// Tries to convert an object to its query representation based on its runtime type.
        /// </summary>
        public static string ToQueryStringDynamic(object obj)
        {
            Contract.Requires(obj != null);

            return ToQueryString((dynamic)obj);
        }
    }
}
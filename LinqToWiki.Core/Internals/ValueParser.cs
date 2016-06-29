using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LinqToWiki.Internals
{
    /// <summary>
    /// Parses the results from an API into a primitive type.
    /// In a way, a couterpart to <see cref="QueryRepresentation"/>.
    /// Used by generated code.
    /// </summary>
    public static class ValueParser
    {
        /// <summary>
        /// “Parses” a string value. Directly returns original string.
        /// </summary>
        public static string ParseString(string value)
        {
            return value;
        }

        /// <summary>
        /// Parses a DateTime. The value of <c>infinity</c> is parsed as <see cref="DateTime.MaxValue"/>.
        /// </summary>
        public static DateTime ParseDateTime(string value)
        {
            if (value == "infinity")
                return DateTime.MaxValue;

            return DateTime.Parse(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parses a namespace.
        /// </summary>
        public static Namespace ParseNamespace(string value, WikiInfo wiki)
        {
            Contract.Requires(wiki != null);

            return wiki.Namespaces[ParseInt32(value)];
        }

        /// <summary>
        /// Parses a boolean. Returns <c>true</c> for empty string, throws an exception otherwise.
        /// Doesn't get called, if the value if <c>false</c>, because then the property is not present
        /// in the result at all.
        /// </summary>
        public static bool ParseBoolean(string value)
        {
            if (value != string.Empty)
                throw new ArgumentException("value");

            return true;
        }

        /// <summary>
        /// Parses an int.
        /// </summary>
        public static int ParseInt32(string value)
        {
            return int.Parse(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parses a long.
        /// </summary>
        public static long ParseInt64(string value)
        {
            return long.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}
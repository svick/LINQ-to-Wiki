using System;
using System.Globalization;

namespace LinqToWiki.Internals
{
    public static class ValueParser
    {
         public static string ParseString(string value)
         {
             return value;
         }

        public static DateTime ParseDateTime(string value)
        {
            if (value == "infinity")
                return DateTime.MaxValue;

            return DateTime.Parse(value, CultureInfo.InvariantCulture);
        }

        public static Namespace ParseNamespace(string value, WikiInfo wiki)
        {
            return wiki.Namespaces[ParseInt32(value)];
        }

        public static bool ParseBoolean(string value)
        {
            if (value != string.Empty)
                throw new ArgumentException("value");

            return true;
        }

        public static int ParseInt32(string value)
        {
            return int.Parse(value, CultureInfo.InvariantCulture);
        }

        public static long ParseInt64(string value)
        {
            return long.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}
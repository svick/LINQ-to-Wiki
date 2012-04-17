using System;
using System.Globalization;

namespace LinqToWiki
{
    public static class QueryRepresentationExtensions
    {
        public static string ToQueryString(IQueryRepresentation queryRepresentation)
        {
            return queryRepresentation.GetQueryRepresentation();
        }

        public static string ToQueryString(this string s)
        {
            return s;
        }

        public static string ToQueryString(this DateTime dateTime)
        {
            return dateTime.ToString("o");
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

        public static string ToQueryString(this Enum e)
        {
            string result = e.ToString();

            if (result == "none")
                result = string.Empty;
            else
                result = result.Replace('_', '-');

            return result;
        }

        public static string ToQueryString(object obj)
        {
            return ToQueryString((dynamic)obj);
        }
    }
}
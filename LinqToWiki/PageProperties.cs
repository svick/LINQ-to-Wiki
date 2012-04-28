using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LinqToWiki
{
    public static class PageProperties<TPage>
    {
        private static readonly Dictionary<string, QueryTypeProperties> PropertiesField =
            typeof(TPage).GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(f => f.Name.EndsWith("Properties"))
                .ToDictionary(
                    f => f.Name.Substring(0, f.Name.Length - "Properties".Length), f => (QueryTypeProperties)f.GetValue(null));

        public static Dictionary<string, QueryTypeProperties> Properties
        {
            get { return PropertiesField; }
        }
    }
}
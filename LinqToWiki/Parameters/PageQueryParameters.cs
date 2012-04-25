using System;
using System.Collections.Generic;
using System.Reflection;

namespace LinqToWiki.Parameters
{
    public class PageQueryParameters
    {
        private PageQueryParameters()
        {}

        public PageQueryParameters(IEnumerable<Tuple<string, string>> baseParameters)
        {
            foreach (var baseParameter in baseParameters)
                Value = new NameValueParameter(Value, baseParameter.Item1, baseParameter.Item2);
        }

        /// <summary>
        /// Linked list of general parameters.
        /// </summary>
        public NameValueParameter Value { get; private set; }

        /// <summary>
        /// Parameters for each prop.
        /// </summary>
        public IEnumerable<PropQueryParameters> PropQueryParametersCollection { get; private set; }

        public PageQueryParameters WithParameters(IEnumerable<PropQueryParameters> parametersCollection)
        {
            if (PropQueryParametersCollection != null)
                throw new InvalidOperationException();

            return new PageQueryParameters { Value = Value, PropQueryParametersCollection = parametersCollection };
        }
    }

    public class PropQueryParameters : QueryParameters
    {
        private PropQueryParameters(string propName, QueryTypeProperties queryTypeProperties)
        {
            PropName = propName;
            QueryTypeProperties = queryTypeProperties;
        }

        public PropQueryParameters(string propName, Type pageType)
        {
            PropName = propName;
            QueryTypeProperties =
                (QueryTypeProperties)pageType.GetField(propName + "Properties", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }

        public string PropName { get; private set; }

        public QueryTypeProperties QueryTypeProperties { get; private set; }

        public PropQueryParameters WithProperties(IEnumerable<string> properties)
        {
            var result = new PropQueryParameters(PropName, QueryTypeProperties);
            CopyTo(result);
            result.Properties = properties;
            return result;
        }
    }
}
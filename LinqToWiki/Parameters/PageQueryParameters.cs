using System;
using System.Collections.Generic;

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
        public PropQueryParameters(string propName)
        {
            PropName = propName;
        }

        public string PropName { get; private set; }

        public PropQueryParameters WithProperties(IEnumerable<string> properties)
        {
            var result = new PropQueryParameters(PropName);
            CopyTo(result);
            result.Properties = properties;
            return result;
        }

        public void CopyFrom(QueryParameters parameters)
        {
            parameters.CopyTo(this);
        }
    }
}
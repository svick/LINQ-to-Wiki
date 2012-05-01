using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Adds a single name-value pair to the current query and returns the result.
        /// The parameter <see cref="value"/> can actually represent multiple values.
        /// </summary>
        public PropQueryParameters AddSingleValue(string name, string value)
        {
            if (value == null)
                return this;

            if (Value != null && Value.Any(v => v.Name == name))
                throw new InvalidOperationException(
                    string.Format("Tried adding value with the name '{0}' that is already present.", name));

            var result = new PropQueryParameters(PropName);
            CopyTo(result);

            result.Value = new NameValueParameter(Value, name, value);
            return result;
        }

    }
}
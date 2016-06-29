using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqToWiki.Parameters
{
    /// <summary>
    /// Query parameters for one prop module.
    /// </summary>
    public class PropQueryParameters : QueryParameters
    {
        public PropQueryParameters(string propName)
        {
            PropName = propName;
        }

        /// <summary>
        /// Name of the module.
        /// </summary>
        public string PropName { get; private set; }

        /// <summary>
        /// Should only the first item be retrieved for each page?
        /// Used by modules that implement <see cref="IFirst"/>, i.e. <c>revisions</c>.
        /// </summary>
        public bool OnlyFirst { get; private set; }

        /// <summary>
        /// Returns a copy of current instance with <see cref="QueryParameters.Properties"/> set.
        /// </summary>
        public PropQueryParameters WithProperties(IEnumerable<string> properties)
        {
            var result = new PropQueryParameters(PropName);
            CopyTo(result);
            result.Properties = properties;
            return result;
        }

        /// <summary>
        /// Returns a copy of current instance with <see cref="OnlyFirst"/> set to <c>true</c>.
        /// </summary>
        /// <returns></returns>
        public PropQueryParameters WithOnlyFirst()
        {
            var result = new PropQueryParameters(PropName);
            CopyTo(result);
            result.OnlyFirst = true;
            return result;
        }

        /// <summary>
        /// Copies the data from <paramref name="parameters"/> into current instance.
        /// </summary>
        public void CopyFrom(QueryParameters parameters)
        {
            parameters.CopyTo(this);
        }

        /// <summary>
        /// Adds a single name-value pair to the current query and returns the result.
        /// The parameter <paramref name="value"/> can actually represent multiple values.
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
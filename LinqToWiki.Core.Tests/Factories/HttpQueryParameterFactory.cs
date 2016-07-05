using LinqToWiki.Download;
// <copyright file="HttpQueryParameterFactory.cs">Copyright ©  2011</copyright>

using System;
using Microsoft.Pex.Framework;

namespace LinqToWiki.Download
{
    /// <summary>A factory for LinqToWiki.Download.HttpQueryParameter instances</summary>
    public static partial class HttpQueryParameterFactory
    {
        /// <summary>A factory for LinqToWiki.Download.HttpQueryParameter instances</summary>
        [PexFactoryMethod(typeof(HttpQueryParameter))]
        public static HttpQueryParameter Create(string name_s, string value_s1)
        {
            HttpQueryParameter httpQueryParameter = new HttpQueryParameter(name_s, value_s1);
            return httpQueryParameter;
        }
    }
}

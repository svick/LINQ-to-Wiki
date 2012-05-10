using System;

namespace LinqToWiki
{
    /// <summary>
    /// Exception representing an error returned by the API.
    /// </summary>
    public class ApiErrorException : Exception
    {
        public ApiErrorException(string code, string info)
            : base(info)
        {
            Code = code;
        }

        /// <summary>
        /// Code of the exception.
        /// </summary>
        public string Code { get; private set; }
    }
}
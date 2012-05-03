using System;

namespace LinqToWiki.Codegen.App
{
    public class ArgumentException : Exception
    {
        public ArgumentException(string message)
            : base(message)
        {}

        public ArgumentException(string message, Exception innerException)
            : base(message, innerException)
        {}
    }
}
using System.IO;

namespace LinqToWiki.Download
{
    public abstract class HttpQueryParameterBase
    {
        public string Name { get; private set; }

        public HttpQueryParameterBase(string name)
        {
            Name = name;
        }
    }

    public class HttpQueryParameter : HttpQueryParameterBase
    {
        public string Value { get; private set; }

        public HttpQueryParameter(string name, string value)
            : base(name)
        {
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("{0}={1}", Name, Value);
        }
    }

    public class HttpQueryFileParameter : HttpQueryParameterBase
    {
        public Stream File { get; private set; }

        public HttpQueryFileParameter(string name, Stream file)
            : base(name)
        {
            File = file;
        }

        public override string ToString()
        {
            return string.Format("{0}=<file>", Name);
        }
    }
}
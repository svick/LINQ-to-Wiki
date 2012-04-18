using System;
using System.Xml.Linq;

namespace LinqToWiki
{
    public class LoginResult
    {
        private LoginResult()
        {}

        public Result Result { get; private set; }

        public string Token { get; private set; }

        public static LoginResult Parse(XElement element)
        {
            var result = new LoginResult();
            result.Result = (Result)Enum.Parse(typeof(Result), (string)element.Attribute("result"));
            result.Token = (string)element.Attribute("token");
            return result;
        }
    }

    public enum Result
    {
        Success,
        NeedToken
    }
}
using System;
using System.Collections.Generic;

namespace LinqToWiki.Codegen.App
{
    class ArgumentsCollection
    {
        readonly List<string> m_positionalArguments = new List<string>();
        readonly Dictionary<char, string> m_namedArguments = new Dictionary<char, string>();

        private ArgumentsCollection()
        {}

        public static ArgumentsCollection Parse(string[] args)
        {
            var result = new ArgumentsCollection();

            int i = 0;
            while (i < args.Length)
            {
                string arg = args[i];
                if (arg.StartsWith("-"))
                {
                    if (arg.Length != 2)
                        throw new InvalidOperationException(string.Format("Invalid argument: '{0}'.", arg));

                    char key = arg[1];
                    string value = args[++i];
                    result.m_namedArguments.Add(key, value);
                }
                else
                    result.m_positionalArguments.Add(arg);
                i++;
            }

            return result;
        }

        public string this[int i]
        {
            get
            {
                if (m_positionalArguments.Count <= i)
                    return null;

                return m_positionalArguments[i];
            }
        }

        public string this[char c]
        {
            get
            {
                string result;
                m_namedArguments.TryGetValue(c, out result);
                return result;
            }
        }
    }
}
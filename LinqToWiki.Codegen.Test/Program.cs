using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqToWiki.Codegen.Test
{
    static class Program
    {
        static void Main()
        {
            var wiki = new Wiki("localhost/wiki/", "api.php");
            Console.WriteLine(wiki);
            //wiki.AddQueryModule("categorymembers");
            wiki.Compile();
        }
    }
}

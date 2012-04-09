using System;

namespace LinqToWiki.Codegen.Test
{
    static class Program
    {
        static void Main()
        {
            var wiki = new Wiki("localhost/wiki/", "api.php");
            wiki.AddQueryModule("categorymembers");
            Console.WriteLine(wiki);
            var result = wiki.Compile("generated.dll");

            Console.WriteLine("Success: {0}", result.Success);

            foreach (var diagnostic in result.Diagnostics)
            {
                Console.WriteLine(diagnostic);
            }
        }
    }
}
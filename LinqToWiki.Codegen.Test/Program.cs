using System;
using System.CodeDom.Compiler;

namespace LinqToWiki.Codegen.Test
{
    static class Program
    {
        static void Main()
        {
            Downloader.LogDownloading = true;

            var wiki = new Wiki("localhost/wiki/", "api.php");
            wiki.AddAllModules();
            wiki.AddAllQueryModules();
            var result = wiki.Compile("generated");

            foreach (CompilerError error in result.Errors)
                Console.WriteLine(error);
        }
    }
}
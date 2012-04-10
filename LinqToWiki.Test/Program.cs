using System;
using System.Linq;

namespace LinqToWiki.Test
{
    static class Program
    {
        static void Main()
        {
            var wiki = new Wiki();
            var results = (from cat in wiki.Query.AllCategories()
                           where cat.min == 1
                           select new { cat.content, cat.size }).ToEnumerable().Take(10).ToList();

            foreach (var result in results)
                Console.WriteLine(result);

            Console.WriteLine("Total: {0}", results.Count);
        }
    }
}
using System;

namespace LinqToWiki.Test
{
    static class Program
    {
        static void Main()
        {
            var wiki = new Wiki();
            var results =
                (from cm in wiki.Query.CategoryMembers("Category:Biography_articles_needing_attention")
                 where cm.type == type.subcat
                 //orderby cm.Timestamp descending
                 select new { cm.pageid, cm.title }).ToList();

            foreach (var result in results)
                Console.WriteLine(result);

            Console.WriteLine("Total: {0}", results.Count);
        }
    }
}
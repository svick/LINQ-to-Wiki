using System;

namespace LinqToWiki.Test
{
    static class Program
    {
        static void Main()
        {
            var wiki = new Wiki();
            var results =
                (from cm in wiki.Query.CategoryMembers("Category:Mathematics")
                 where cm.Namespace == Namespace.Article
                 orderby cm.Timestamp descending
                 select new { cm.PageId, cm.Title, cm.SortKey }).ToList();

            foreach (var result in results)
                Console.WriteLine(result);
        }
    }
}
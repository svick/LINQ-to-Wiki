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
                 select new { cm.Id, cm.Title, cm.SortKey }).ToList();
        }
    }
}
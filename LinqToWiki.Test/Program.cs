using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqToWiki.Test
{
    static class Program
    {
        static void Main()
        {
            Downloader.LogDownloading = true;

            var wiki = new Wiki();
            AllUsers(wiki);
        }

        private static void AllCategories(Wiki wiki)
        {
            var results = (from cat in wiki.Query.AllCategories()
                           where cat.min == 1
                           orderby cat
                           select new { cat.value, cat.size, cat.subcats })
                .ToEnumerable().Take(10).ToList();

            Write(results);
        }

        private static void AllImages(Wiki wiki)
        {
            var results = (from image in wiki.Query.AllImages()
                           orderby image
                           select new { image.name, image.pagecount, comment = image.comment.Substring(0, Math.Min(20, image.comment.Length)) })
                .ToEnumerable().Take(10).ToList();

            Write(results);
        }

        private static void AllLinks(Wiki wiki)
        {
            var results = (from link in wiki.Query.AllLinks()
                           where link.ns == Namespace.Talk
                           select link)
                .ToEnumerable().Take(10).ToList();

            Write(results);
        }

        private static void AllMessages(Wiki wiki)
        {
            var result = (from m in wiki.Query.AllMessages()
                          //where m.messages == "about"
                          where m.customised == customised.modified
                          select m)
                .ToEnumerable().Take(10).ToList();

            Write(result);
        }

        private static void AllPages(Wiki wiki)
        {
            var result = (from page in wiki.Query.AllPages()
                          where page.prtype == prtype.edit
                          where page.prlevel == prlevel.none
                          select page.title)
                .ToEnumerable().Take(10).ToList();

            Write(result);
        }

        private static void AllUsers(Wiki wiki)
        {
            var result = (from user in wiki.Query.AllUsers()
                          where user.rights == rights.move_subpages
                          orderby user descending
                          select new { user.name })
                .ToEnumerable().Take(20).ToList();

            Write(result);
        }

        private static void Write<T>(ICollection<T> results)
        {
            foreach (var result in results)
                Console.WriteLine(result);

            Console.WriteLine("Total: {0}", results.Count);
        }
    }
}
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

            //var wiki = new Wiki();
            var wiki = new Wiki("localhost/wiki/", "api.php");
            Login(wiki, "Svick", "heslo");
        }

        private static void Login(Wiki wiki, string name, string password)
        {
            var result = wiki.Login(name, password);

            if (result.result == LinqToWiki.result.NeedToken)
                result = wiki.Login(name, password, token: result.token);

            if (result.result != LinqToWiki.result.Success)
                throw new Exception(result.result.ToString());
        }

        private static void AllCategories(Wiki wiki)
        {
            var results = (from cat in wiki.Query.AllCategories()
                           where cat.min == 1
                           orderby cat
                           select new { cat.value, cat.size, cat.subcats })
                .ToEnumerable().Take(10);

            Write(results);
        }

        private static void AllImages(Wiki wiki)
        {
            var results = (from image in wiki.Query.AllImages()
                           orderby image
                           select
                               new
                               {
                                   image.name,
                                   image.pagecount,
                                   comment = image.comment.Substring(0, Math.Min(20, image.comment.Length))
                               })
                .ToEnumerable().Take(10);

            Write(results);
        }

        private static void AllLinks(Wiki wiki)
        {
            var results = (from link in wiki.Query.AllLinks()
                           where link.ns == Namespace.Talk
                           where link.unique == false
                           select link)
                .ToEnumerable().Take(10);

            Write(results);
        }

        private static void AllMessages(Wiki wiki)
        {
            var result = (from m in wiki.Query.AllMessages()
                          //where m.messages == "about"
                          where m.customised == customised.modified
                          select m)
                .ToEnumerable().Take(10);

            Write(result);
        }

        private static void AllPages(Wiki wiki)
        {
            var result = (from page in wiki.Query.AllPages()
                          where page.prtype == prtype.edit
                          where page.prlevel == prlevel.none
                          select page.title)
                .ToEnumerable().Take(10);

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

        private static void Backlinks(Wiki wiki)
        {
            var result = (from bl in wiki.Query.Backlinks("User:Svick")
                          where bl.ns == Namespace.Project
                          select bl.title)
                .ToEnumerable().Take(10);

            Write(result);
        }

        private static void Blocks(Wiki wiki)
        {
            var result = (from block in wiki.Query.Blocks()
                          where block.end == DateTime.UtcNow.AddMinutes(-10)
                          where block.show == show.not_ip
                          where block.show == show.not_temp
                          select new { block.user, block.timestamp, block.@by })
                .ToEnumerable().Take(10);

            Write(result);
        }

        private static void CategoryMembers(Wiki wiki)
        {
            var result = (from cm in wiki.Query.CategoryMembers()
                          where cm.title == "Category:Query languages"
                          //orderby cm.sortkey
                          select new { cm.title, cm.sortkeyprefix, cm.type })
                .ToEnumerable().Take(10);

            Write(result);
        }

        private static void Write<T>(IEnumerable<T> results)
        {
            var array = results.ToArray();

            foreach (var result in array)
                Console.WriteLine(result);

            Console.WriteLine("Total: {0}", array.Length);
        }
    }
}
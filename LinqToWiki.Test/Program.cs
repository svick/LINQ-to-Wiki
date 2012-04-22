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

            /**/
            var wiki = new Wiki();
            /*/
            var wiki = new Wiki("localhost/wiki/", "api.php");
            Login(wiki, "Svick", "heslo");
            /**/
            RecentChanges(wiki);
        }

        private static void Login(Wiki wiki, string name, string password)
        {
            var result = wiki.login(name, password);

            if (result.result == LinqToWiki.result.NeedToken)
                result = wiki.login(name, password, token: result.token);

            if (result.result != LinqToWiki.result.Success)
                throw new Exception(result.result.ToString());
        }

        private static void AllCategories(Wiki wiki)
        {
            var results = (from cat in wiki.Query.allcategories()
                           where cat.min == 1
                           orderby cat
                           select new { cat.value, cat.size, cat.subcats })
                .ToEnumerable().Take(10);

            Write(results);
        }

        private static void AllImages(Wiki wiki)
        {
            var results = (from image in wiki.Query.allimages()
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
            var results = (from link in wiki.Query.alllinks()
                           where link.ns == Namespace.Talk
                           where link.unique == false
                           select link)
                .ToEnumerable().Take(10);

            Write(results);
        }

        private static void AllMessages(Wiki wiki)
        {
            var result = (from m in wiki.Query.allmessages()
                          //where m.messages == "about"
                          where m.customised == customised.modified
                          select m)
                .ToEnumerable().Take(10);

            Write(result);
        }

        private static void AllPages(Wiki wiki)
        {
            var result = (from page in wiki.Query.allpages()
                          where page.prtype == prtype.edit
                          where page.prlevel == prlevel.none
                          select page.title)
                .ToEnumerable().Take(10);

            Write(result);
        }

        private static void AllUsers(Wiki wiki)
        {
            var result = (from user in wiki.Query.allusers()
                          where user.rights == rights.move_subpages
                          orderby user descending
                          select new { user.name, user.userid })
                .ToEnumerable().Take(20).ToList();

            Write(result);
        }

        private static void Backlinks(Wiki wiki)
        {
            var result = (from bl in wiki.Query.backlinks("User:Svick")
                          where bl.ns == Namespace.Project
                          select bl.title)
                .ToEnumerable().Take(10);

            Write(result);
        }

        private static void Blocks(Wiki wiki)
        {
            var result = (from block in wiki.Query.blocks()
                          where block.end == DateTime.UtcNow.AddMinutes(-10)
                          where block.show == show.not_ip
                          where block.show == show.not_temp
                          select new { block.user, block.timestamp, block.@by })
                .ToEnumerable().Take(10);

            Write(result);
        }

        private static void CategoryMembers(Wiki wiki)
        {
            var result = (from cm in wiki.Query.categorymembers()
                          where cm.title == "Category:Query languages"
                          //orderby cm.sortkey
                          select new { cm.title, cm.sortkeyprefix, cm.type })
                .ToEnumerable().Take(10);

            Write(result);
        }

        private static void DeletedRevs(Wiki wiki)
        {
            var result = (from dr in wiki.Query.deletedrevs()
                          where dr.user == "Svick"
                          select dr.title)
                .ToEnumerable();

            Write(result);
        }

        private static void EmbeddedIn(Wiki wiki)
        {
            var result = (from ei in wiki.Query.embeddedin("Template:WikiProject cleanup listing")
                          where ei.filterredir == filterredir.nonredirects
                          select new { ei.title, ei.redirect })
                .ToEnumerable().Take(10);

            Write(result);
        }

        private static void ExtUrlUsage(Wiki wiki)
        {
            var result = (from eu in wiki.Query.exturlusage()
                          where eu.query == "toolserver.org/~svick"
                          select new { eu.title, eu.url })
                .ToEnumerable().Take(10);

            Write(result);
        }

        private static void ImageUsage(Wiki wiki)
        {
            var result = wiki.Query.imageusage("File:Indiafilm.svg")
                .ToEnumerable().Take(10);

            Write(result);
        }

        private static void IwBacklinks(Wiki wiki)
        {
            var result = (from ib in wiki.Query.iwbacklinks()
                          where ib.prefix == "wikia"
                          select ib)
                .ToEnumerable().Where(ib => ib.iwtitle != "").Take(10);

            Write(result);
        }

        private static void LangBacklinks(Wiki wiki)
        {
            var result = (from lb in wiki.Query.langbacklinks()
                          where lb.lang == "cs"
                          where lb.title == "Wikipedie:Cykly v kategoriích"
                          select lb.title)
                .ToEnumerable().Take(10);

            Write(result);
        }

        private static void LogEvents(Wiki wiki)
        {
            var result = (from le in wiki.Query.logevents()
                          where le.action == action.block_block
                          orderby le
                          select new { le.title, le.user, le.comment, le.timestamp })
                .ToEnumerable().Take(10);

            Write(result);
        }

        private static void ProtectedTitles(Wiki wiki)
        {
            var result = from pt in wiki.Query.protectedtitles()
                         where pt.level == level.autoconfirmed
                         orderby pt descending
                         select pt;

            Write(result);
        }

        private static void QueryPage(Wiki wiki)
        {
            var result = from qp in wiki.Query.querypage(page.Uncategorizedpages)
                         select qp;

            Write(result);
        }

        private static void Random(Wiki wiki)
        {
            var result = from rp in wiki.Query.random()
                         select rp.title;

            Write(result);
        }

        private static void RecentChanges(Wiki wiki)
        {
            var result = from rc in wiki.Query.recentchanges()
                         where rc.type == type4.@new
                         where rc.show == show2.bot
                         orderby rc
                         select new { rc.title, rc.comment, rc.@new, rc.bot, rc.timestamp };

            Write(result);
        }

        private static void Write<TSource, TResult>(WikiQueryResult<TSource, TResult> results)
        {
            Write(results.ToEnumerable().Take(10));
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
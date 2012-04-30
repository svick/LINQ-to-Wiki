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
            Images(AllPagesSource(wiki));
        }

        private static void Login(Wiki wiki, string name, string password)
        {
            var result = wiki.login(name, password);

            if (result.result == loginresult.NeedToken)
                result = wiki.login(name, password, token: result.token);

            if (result.result != loginresult.Success)
                throw new Exception(result.result.ToString());
        }

        private static PagesSource<Page> TitlePages(Wiki wiki)
        {
            return wiki.CreateTitlesSource(
                "User:Svick", "User talk:Svick/WikiProject cleanup listing", "Nonfoobar", "", "Special:Version");
        }

        private static PagesSource<Page> PageIdPages(Wiki wiki)
        {
            return wiki.CreatePageIdsSource(21061255, 29516325);
        }

        private static PagesSource<Page> RevIdsPages(Wiki wiki)
        {
            return wiki.CreateRevIdsSource(489663021, 489132906);
        }

        private static PagesSource<Page> AllPagesSource(Wiki wiki)
        {
            return wiki.Query.allpages()
                .Where(p => p.filterredir == allpagesfilterredir.nonredirects)
                .Pages;
        }

        private static PagesSource<Page> CategoryMembersSource(Wiki wiki)
        {
            return (from cm in wiki.Query.categorymembers()
                    where cm.title == "Category:Query languages"
                          && cm.type == categorymemberstype.subcat
                    select cm).Pages;
        }

        private static void PageResultProps(PagesSource<Page> pages)
        {
            var source = pages
                .Select(
                    p =>
                    PageResult.Create(
                        p.info,
                        p.categories()
                            .Where(c => c.show == categoriesshow.not_hidden)
                            .OrderByDescending(c => c)
                            .Select(c => new { c.title, c.sortkeyprefix })
                            .ToEnumerable())
                );

            Write(source);
        }

        private static void Categories(PagesSource<Page> pages)
        {
            var source = pages
                .Select(
                    p =>
                    new
                    {
                        p.info,
                        categories =
                        p.categories()
                        .Where(c => c.show == categoriesshow.not_hidden)
                        .Select(c => new { c.title , c.sortkeyprefix })
                        .ToEnumerable()
                        .Take(1)
                    }
                );

            foreach (var page in source.ToEnumerable().Take(10))
            {
                Console.WriteLine(page.info.title);
                foreach (var item in page.categories.Take(10))
                    Console.WriteLine("  " + item);
            }
        }

        private static void DuplicateFiles(PagesSource<Page> pages)
        {
            var source = pages
                .Select(
                    p =>
                    new
                    {
                        p.info.title,
                        duplicatefiles = p.duplicatefiles().Select(d => d.name)
                        .ToEnumerable(),
                    }
                );

            foreach (var page in source.ToEnumerable().Where(i => i.duplicatefiles.Any()).Take(5))
            {
                Console.WriteLine(page.title);
                foreach (var item in page.duplicatefiles.Take(10))
                    Console.WriteLine("  " + item);
            }
        }

        private static void ExtLinks(PagesSource<Page> pages)
        {
            var source = pages
                .Select(
                    p =>
                    PageResult.Create(
                        p.info,
                        p.extlinks()
                            .Where(l => l.query == "toolserver.org")
                            .Select(l => l.value)
                            .ToEnumerable())
                );

            Write(source);
        }

        private static void ImageInfo(PagesSource<Page> pages)
        {
            var source = pages
                .Select(
                    p =>
                    PageResult.Create(
                        p.info,
                        p.imageinfo()
                            .Select(i => new { i.timestamp, i.comment })
                            .ToEnumerable())
                );

            Write(source);
        }

        private static void Images(PagesSource<Page> pages)
        {
            var source = pages
                .Select(
                    p =>
                    PageResult.Create(
                        p.info,
                        p.images().ToEnumerable())
                );

            Write(source);
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
                          where m.messages == new[] { "aboutsite", "abusefilter" }
                          where m.customised == allmessagescustomised.modified
                          select m)
                .ToEnumerable().Take(10);

            Write(result);
        }

        private static void AllPages(Wiki wiki)
        {
            var result = (from page in wiki.Query.allpages()
                          where page.prtype == allpagesprtype.edit
                          where page.prlevel == allpagesprlevel.none
                          select page.title)
                .ToEnumerable().Take(10);

            Write(result);
        }

        private static void AllUsers(Wiki wiki)
        {
            var result = (from user in wiki.Query.allusers()
                          where user.rights == allusersrights.move_subpages
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
                          where block.show == blocksshow.not_ip
                          where block.show == blocksshow.not_temp
                          select new { block.user, block.timestamp, block.@by })
                .ToEnumerable().Take(10);

            Write(result);
        }

        private static void CategoryMembers(Wiki wiki)
        {
            var result = (from cm in wiki.Query.categorymembers()
                          where cm.title == "Category:Query languages"
                                && cm.ns == Namespace.Category // or new[] { Namespace.Category }
                                && cm.startsortkeyprefix == "xml"
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
                          where ei.filterredir == embeddedinfilterredir.nonredirects
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
                          where le.action == logeventsaction.block_block
                          orderby le
                          select new { le.title, le.user, le.comment, le.timestamp })
                .ToEnumerable().Take(10);

            Write(result);
        }

        private static void ProtectedTitles(Wiki wiki)
        {
            var result = from pt in wiki.Query.protectedtitles()
                         where pt.level == protectedtitleslevel.autoconfirmed
                         orderby pt descending
                         select pt;

            Write(result);
        }

        private static void QueryPage(Wiki wiki)
        {
            var result = from qp in wiki.Query.querypage(querypagepage.Uncategorizedpages)
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
                         where rc.type == recentchangestype2.@new
                         where rc.show == recentchangesshow.bot
                         orderby rc
                         select new { rc.title, rc.comment, rc.@new, rc.bot, rc.timestamp };

            Write(result);
        }

        private static void Search(Wiki wiki)
        {
            var result = from s in wiki.Query.search("LINQ")
                         select new { s.title, snippet = s.snippet.Substring(0, 30) };

            Write(result);
        }

        private static void Tags(Wiki wiki)
        {
            var result = wiki.Query.tags().Select(tag => new { tag.name, tag.displayname, tag.hitcount });

            Write(result);
        }

        private static void UserContribs(Wiki wiki)
        {
            var result = from uc in wiki.Query.usercontribs()
                         where uc.userprefix == "Svick"
                         where uc.start == DateTime.Now.AddDays(-2)
                         orderby uc
                         select new { uc.user, uc.title, uc.timestamp, uc.comment };

            Write(result);
        }

        private static void Users(Wiki wiki)
        {
            var result = from u in wiki.Query.users()
                         where u.users == new[] { "Svick", "SvickBOT" }
                         select u;

            Write(result);
        }

        private static void Watchlist(Wiki wiki)
        {
            var result = from wl in wiki.Query.watchlist()
                         where wl.show == (watchlistshow.bot | watchlistshow.not_minor)
                         where !wl.allrev
                         select new { wl.title, wl.user, wl.timestamp, wl.comment };

            Write(result);
        }

        private static void WatchlistRaw(Wiki wiki)
        {
            var result = from wr in wiki.Query.watchlistraw()
                         select null;

            Write(result);
        }

        private static void Write<T>(WikiQueryPageResult<PageResult<T>> source)
        {
            foreach (var page in source.ToEnumerable().Take(10))
            {
                Console.WriteLine(page.Info.title);
                foreach (var item in page.Data.Take(10))
                    Console.WriteLine("  " + item);
            }
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
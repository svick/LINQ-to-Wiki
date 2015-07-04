using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LinqToWiki.Download;
using LinqToWiki.Generated;

namespace LinqToWiki.Samples
{
    static class Program
    {
        private static void Main()
        {
            Downloader.LogDownloading = true;

            var wiki = new Wiki("LinqToWiki.Samples", "en.wikipedia.org", "/w/api.php");
            // Login(wiki, "username", "password");

            Compare(wiki);
            // Categories(TitlePages(wiki));
        }

        #region Simple methods

        private static void Block(Wiki wiki)
        {
            var token = wiki.tokens(new[] { tokenstype.block }).blocktoken;
            wiki.block("Test", token);
        }

        private static void Compare(Wiki wiki)
        {
            var result = wiki.compare(fromrev: 486474789, torev: 487063697);
            Console.WriteLine(result.value);
        }

        private static void Delete(Wiki wiki)
        {
            string token = wiki.tokens(new[] { tokenstype.delete }).deletetoken;
            wiki.delete(token, "Test");
        }

        private static void Edit(Wiki wiki)
        {
            var token = wiki.tokens(new[] { tokenstype.edit }).edittoken;
            var result = wiki.edit(
                title: "Wikipedia talk:Sandbox", section: "new", sectiontitle: "Hello", text: "Hello world! ~~~~",
                summary: "greeting the world", token: token);
            Console.WriteLine(result);
        }

        private static void EmailUser(Wiki wiki)
        {
            var token = wiki.tokens(new[] { tokenstype.email }).emailtoken;
            var result = wiki.emailuser("User:Svick", "Hello", "Mail from LinqToWiki", token);
            Console.WriteLine(result);
        }

        private static void ExpandTemplates(Wiki wiki)
        {
            var result = wiki.expandtemplates("{{start date and age|1988|5|20|df=yes}}");
            Console.WriteLine(result.value);
        }

        private static void FileRevert(Wiki wiki)
        {
            var fileName = "TOR1.jpeg";
            var info = wiki.CreateTitlesSource("File:" + fileName)
                .Select(
                    f =>
                    new
                    {
                        token = f.info.edittoken,
                        archiveNames = f.imageinfo().Select(i => i.archivename).ToEnumerable()
                    })
                .ToEnumerable().Single();
            var archiveName = info.archiveNames.ElementAt(1);

            var result = wiki.filerevert(fileName, archiveName, "I don't like this version of the image.", info.token);

            Console.WriteLine(result);
        }

        private static void Import(Wiki wiki)
        {
            var token = wiki.tokens(new[] { tokenstype.import }).importtoken;

            var result = wiki.import(
                token, "imported some pages", interwikisource: importinterwikisource.meta,
                interwikipage: "User:Svick/Sandbox", templates: true);

            Write(result);
        }

        private static void Login(Wiki wiki, string name, string password)
        {
            var result = wiki.login(name, password);

            if (result.result == loginresult.NeedToken)
                result = wiki.login(name, password, token: result.token);

            if (result.result != loginresult.Success)
                throw new Exception(result.result.ToString());
        }

        private static void Logout(Wiki wiki)
        {
            wiki.logout();
        }

        private static void Move(Wiki wiki)
        {
            var token = wiki.tokens(new[] { tokenstype.move }).movetoken;

            var result = wiki.move(from: "Test2", to: "Test", token: token);
            Console.WriteLine(result);
        }

        private static void Options(Wiki wiki)
        {
            var tokens = wiki.tokens(new[] { tokenstype.options });
            wiki.options(tokens.optionstoken, change: new[] { "skin=monobook" });
        }

        private static void Patrol(Wiki wiki)
        {
            var recentChange = wiki.Query.recentchanges()
                .Where(rc => rc.show == recentchangesshow.not_patrolled)
                .Where(rc => rc.token == recentchangestoken.patrol)
                .ToEnumerable()
                .First();

            Console.WriteLine(recentChange);

            var result = wiki.patrol(recentChange.patroltoken, recentChange.rcid);

            Console.WriteLine(result);
        }

        private static void Protect(Wiki wiki)
        {
            var token = wiki.tokens(new[] { tokenstype.protect }).protecttoken;

            var result = wiki.protect(new[] { "edit=autoconfirmed", "move=sysop" }, token, "Test");
            Console.WriteLine(result);
        }

        private static void Purge(Wiki wiki)
        {
            var result = wiki.purge(titles: new[] { "Test", "Test2", "Test3" }, forcelinkupdate: true);
            Write(result);
        }

        private static void Rollback(Wiki wiki)
        {
            var title = "Test";
            var tokens =
                wiki.CreateTitlesSource(title).Select(
                    p =>
                    p.revisions()
                        .Where(r => r.token == revisionstoken.rollback)
                        .Select(r => r.rollbacktoken).
                        ToEnumerable())
                    .ToEnumerable().Single();
            var result = wiki.rollback(title, "127.0.0.1", tokens.First());
            Console.WriteLine(result);
        }

        private static void Tokens(Wiki wiki)
        {
            var tokens = wiki.tokens(
                new[]
                {
                    tokenstype.block, tokenstype.delete, tokenstype.protect, tokenstype.watch, tokenstype.unblock,
                    tokenstype.move, tokenstype.patrol, tokenstype.import, tokenstype.edit
                });
            Console.WriteLine(tokens);
        }

        private static void Unblock(Wiki wiki)
        {
            var token = wiki.tokens(new[] { tokenstype.unblock }).unblocktoken;
            var result = wiki.unblock(user: "Test", token: token, reason: "I don't hate you anymore.");
            Console.WriteLine(result);
        }

        private static void Undelete(Wiki wiki)
        {
            var title = "Ke smazání";
            var deletedPage = wiki.Query.deletedrevs()
                .Where(p => p.ns == Namespace.Article)
                .ToEnumerable()
                .Single(p => p.title == title);

            var result = wiki.undelete(title, deletedPage.token, "Just because");
            Console.WriteLine(result);
        }

        private static void Upload(Wiki wiki)
        {
            var token = wiki.tokens(new[] { tokenstype.edit }).edittoken;
            var result = wiki.upload(
                token,
                "Flower.jpeg",
                url: "http://upload.wikimedia.org/wikipedia/commons/4/4e/Hymensoporum_flavum_flowers.jpg");

            Console.WriteLine(result);
        }

        private static void UploadFile(Wiki wiki)
        {
            var token = wiki.tokens(new[] { tokenstype.edit }).edittoken;
            using (var stream = File.OpenRead(@"C:\Users\Public\Pictures\Sample Pictures\Chrysanthemum.jpg"))
            {
                var result = wiki.upload(token, "Flower.jpeg", file: stream, ignorewarnings: true);

                Console.WriteLine(result);
            }
        }

        private static void Watch(Wiki wiki)
        {
            var token = wiki.tokens(new[] { tokenstype.watch }).watchtoken;
            var result = wiki.watch(token, titles: new[]{ "Test" }, unwatch: false);
            Console.WriteLine(result);
        }

        #endregion

        #region Page sources

        private static PagesSource<Page> TitlePages(Wiki wiki)
        {
            return wiki.CreateTitlesSource(
                "User:Svick", "User talk:Svick/WikiProject cleanup listing", "Nonfoobar", "Special:Version");
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

        #endregion

        #region Page source methods

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
                        .Select(c => new { c.title, c.sortkeyprefix })
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

        private static void CategoryInfo(PagesSource<Page> pages)
        {
            var source = pages.Select(p => new { p.info.title, p.categoryinfo.pages, p.categoryinfo.hidden })
                .ToEnumerable();

            Write(source);
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

        private static void IwLinks(PagesSource<Page> pages)
        {
            var source = pages
                .Select(p => PageResult.Create(p.info, p.iwlinks().ToEnumerable()))
                .ToEnumerable()
                .Where(p => p.Data.Any());

            Write(source);
        }

        private static void LangLinks(PagesSource<Page> pages)
        {
            var source = pages
                .Select(
                    p => PageResult.Create(
                        p.info,
                        p.langlinks().Where(l => l.url).ToEnumerable()))
                .ToEnumerable();

            Write(source);
        }

        private static void Links(PagesSource<Page> pages)
        {
            var source = pages
                .Select(p => PageResult.Create(p.info, p.links().Select(l => l.title).ToEnumerable()));

            Write(source);
        }

        private static void Revisions(PagesSource<Page> pages)
        {
	        var source =
		        pages.Select(
			        p =>
			        PageResult.Create(p.info, p.revisions().OrderBy(r => r).Select(r => r.user).ToEnumerable().First()))
			        .ToEnumerable();
            Write(source);
        }

        private static void Templates(PagesSource<Page> pages)
        {
            var source = pages
                .Select(
                    p =>
                    PageResult.Create(
                        p.info,
                        p.templates()
                            .Where(t => t.ns == new[] { Namespace.Project, Namespace.User, Namespace.Article })
                            .Select(t => t.title)
                            .ToEnumerable()))
                .ToEnumerable()
                .Where(p => p.Data.Any());

            Write(source);
        }

        #endregion

        #region List methods

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
                          where page.@from == "S"
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
            var result = (from bl in wiki.Query.backlinks()
                          where bl.title == "User:Svick"
                                && bl.ns == Namespace.Project
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
                          select dr)
                .ToEnumerable();

            Write(result);
        }

        private static void EmbeddedIn(Wiki wiki)
        {
            var result = (from ei in wiki.Query.embeddedin()
                          where ei.title == "Template:WikiProject cleanup listing"
                                && ei.filterredir == embeddedinfilterredir.nonredirects
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
            var result = wiki.Query.imageusage()
                .Where(iu => iu.title == "File:Indiafilm.svg")
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
                         where uc.user == "Svick"
                         //orderby uc
                         select new { uc.title, uc.@new };

            Write(result.ToEnumerable().Where(uc => uc.@new).Take(2));
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
                         where wl.show == new[] { watchlistshow.bot, watchlistshow.not_minor }
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

        #endregion

        #region Lots of items in TitlesSource

        private static void BigTitlesSource(Wiki wiki)
        {
            var pageTitles = wiki.Query.allpages()
                .Where(p => p.filterredir == allpagesfilterredir.nonredirects)
                .Select(p => p.title)
                .ToEnumerable();
            var pagesSource = wiki.CreateTitlesSource(pageTitles);

            var lotsOfCategories = pagesSource
                .Select(p => new { p.info.title, categories = p.categories().Select(c => 1).ToEnumerable().Count() })
                .ToEnumerable()
                .Where(p => p.categories >= 20)
                .Take(5);

            Write(lotsOfCategories);
        }

        #endregion

        #region Real-world complex query

        // http://en.wikipedia.org/wiki/Wikipedia_talk:Categorization/Archive_14#Unused_categories
        private static void EmptyCategoriesFaster(Wiki wiki)
        {
            var result = wiki.Query
                .allcategories()
                .Where(c => c.max == 0)
                .Pages
                .Select(
                    c => new
                    {
                        c.info.title,
                        c.info.missing,
                        softRedirectCategory =
                             c.categories()
                             .Where(cc => cc.categories == "Category:Wikipedia soft redirected categories")
                             .Select(cc => cc.title)
                             .ToEnumerable()
                    })
                .ToEnumerable()
                .Where(c => !c.missing && !c.softRedirectCategory.Any())
                .Select(c => c.title)
                .Take(10);

            Write(result);
        }

        private static void EmptyCategoriesSlower(Wiki wiki)
        {
            var result = wiki.Query
                .allpages()
                .Where(p => p.ns == Namespace.Category)
                .Pages
                .Select(
                    p =>
                    new
                    {
                        p.info.title,
                        p.categoryinfo,
                        softRedirectCategory =
                            p.categories()
                            .Where(c => c.categories == "Category:Wikipedia soft redirected categories")
                            .Select(c => c.title)
                            .ToEnumerable()
                    })
                .ToEnumerable()
                .Where(c => (c.categoryinfo == null || c.categoryinfo.size == 0) && !c.softRedirectCategory.Any())
                .Select(c => c.title)
                .Take(10);

            Write(result);
        }

        #endregion

        #region Helper methods

        private static void Write<T>(WikiQueryPageResult<PageResult<T>> source)
        {
            Write(source.ToEnumerable());
        }

        private static void Write<T>(IEnumerable<PageResult<T>> source)
        {
            foreach (var page in source.Take(10))
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

        #endregion
    }
}
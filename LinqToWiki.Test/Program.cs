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
            AllImages(wiki);
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
                           select new { image.name, image.size })
                .ToEnumerable().Take(10).ToList();

            Write(results);
        }

        private static void Write<T>(ICollection<T> results)
        {
            foreach (var result in results)
                Console.WriteLine(result);

            Console.WriteLine("Total: {0}", results.Count);
        }
    }
}
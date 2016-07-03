using LinqToWiki.Internals;
using LinqToWiki.Download;
// <copyright file="DownloaderFactory.cs">Copyright ©  2011</copyright>

using System;
using Microsoft.Pex.Framework;

namespace LinqToWiki.Download
{
    /// <summary>A factory for LinqToWiki.Download.Downloader instances</summary>
    public static partial class DownloaderFactory
    {
        /// <summary>A factory for LinqToWiki.Download.Downloader instances</summary>
        [PexFactoryMethod(typeof(Downloader))]
        public static Downloader Create(WikiInfo wiki_wikiInfo)
        {
            Downloader downloader = new Downloader(wiki_wikiInfo);
            return downloader;
        }
    }
}

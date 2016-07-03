using System.Collections.Generic;
using System.Xml.Linq;
using LinqToWiki.Internals;
// <copyright file="DownloaderTest.cs">Copyright ©  2011</copyright>

using System;
using LinqToWiki.Download;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToWiki.Download.Tests
{
    [TestClass]
    [PexClass(typeof(Downloader))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class DownloaderTest
    {

        [PexMethod]
        public Downloader Constructor(WikiInfo wiki)
        {
            Downloader target = new Downloader(wiki);
            return target;
            // TODO: add assertions to method DownloaderTest.Constructor(WikiInfo)
        }

        [PexMethod]
        public XDocument Download([PexAssumeUnderTest]Downloader target, IEnumerable<HttpQueryParameterBase> parameters)
        {
            XDocument result = target.Download(parameters);
            return result;
            // TODO: add assertions to method DownloaderTest.Download(Downloader, IEnumerable`1<HttpQueryParameterBase>)
        }
    }
}

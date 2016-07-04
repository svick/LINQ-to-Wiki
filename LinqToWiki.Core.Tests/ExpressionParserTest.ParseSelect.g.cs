﻿using Microsoft.Pex.Framework.Generated;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using LinqToWiki.Parameters;
// <copyright file="ExpressionParserTest.ParseSelect.g.cs">Copyright ©  2011</copyright>
// <auto-generated>
// This file contains automatically generated tests.
// Do not modify this file manually.
// 
// If the contents of this file becomes outdated, you can delete it.
// For example, if it no longer compiles.
// </auto-generated>
using System;

namespace LinqToWiki.Expressions.Tests
{
    public partial class ExpressionParserTest
    {

[TestMethod]
[PexGeneratedBy(typeof(ExpressionParserTest))]
[ExpectedException(typeof(ArgumentNullException))]
public void ParseSelectThrowsArgumentNullException506()
{
    QueryParameters<int, int> queryParameters;
    queryParameters = this.ParseSelect<int, int>
                          ((Expression<Func<int, int>>)null, (QueryParameters<int, int>)null);
}
    }
}

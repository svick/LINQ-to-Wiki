using System.Collections.Generic;
// <copyright file="TupleListT1T2Test.cs">Copyright ©  2011</copyright>

using System;
using LinqToWiki.Collections;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToWiki.Collections.Tests
{
    [TestClass]
    [PexClass(typeof(TupleList<, >))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class TupleListT1T2Test
    {

        [PexGenericArguments(typeof(int), typeof(int))]
        [PexMethod]
        public void Add<T1, T2>(
            [PexAssumeUnderTest]TupleList<T1, T2> target,
            T1 item1,
            T2 item2
        )
        {
            target.Add(item1, item2);
            // TODO: add assertions to method TupleListT1T2Test.Add(TupleList`2<!!0,!!1>, !!0, !!1)
        }

        [PexGenericArguments(typeof(int), typeof(int))]
        [PexMethod]
        [PexAllowedException(typeof(ArgumentNullException))]
        public TupleList<T1, T2> Constructor01<T1, T2>(IEnumerable<Tuple<T1, T2>> collection)
        {
            TupleList<T1, T2> target = new TupleList<T1, T2>(collection);
            return target;
            // TODO: add assertions to method TupleListT1T2Test.Constructor01(IEnumerable`1<Tuple`2<!!0,!!1>>)
        }

        [PexGenericArguments(typeof(int), typeof(int))]
        [PexMethod]
        public TupleList<T1, T2> Constructor<T1, T2>()
        {
            TupleList<T1, T2> target = new TupleList<T1, T2>();
            return target;
            // TODO: add assertions to method TupleListT1T2Test.Constructor()
        }

        [PexGenericArguments(typeof(int), typeof(int))]
        [PexMethod]
        public T2 ItemGet<T1, T2>([PexAssumeUnderTest]TupleList<T1, T2> target, T1 key)
        {
            T2 result = target[key];
            return result;
            // TODO: add assertions to method TupleListT1T2Test.ItemGet(TupleList`2<!!0,!!1>, !!0)
        }

        [PexGenericArguments(typeof(int), typeof(int))]
        [PexMethod]
        public void ItemSet<T1, T2>(
            [PexAssumeUnderTest]TupleList<T1, T2> target,
            T1 key,
            T2 value
        )
        {
            target[key] = value;
            // TODO: add assertions to method TupleListT1T2Test.ItemSet(TupleList`2<!!0,!!1>, !!0, !!1)
        }
    }
}

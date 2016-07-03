using System.Collections.Generic;
// <copyright file="DictionaryDictionaryTKey1TKey2TValueTest.cs">Copyright ©  2011</copyright>

using System;
using LinqToWiki.Collections;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToWiki.Collections.Tests
{
    [TestClass]
    [PexClass(typeof(DictionaryDictionary<, , >))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class DictionaryDictionaryTKey1TKey2TValueTest
    {

        [PexGenericArguments(typeof(int), typeof(int), typeof(int))]
        [PexMethod]
        [PexAllowedException(typeof(ArgumentException))]
        public void Add<TKey1, TKey2, TValue>(
            [PexAssumeUnderTest]DictionaryDictionary<TKey1, TKey2, TValue> target,
            TKey1 key1,
            TKey2 key2,
            TValue value
        )
        {
            target.Add(key1, key2, value);
            // TODO: add assertions to method DictionaryDictionaryTKey1TKey2TValueTest.Add(DictionaryDictionary`3<!!0,!!1,!!2>, !!0, !!1, !!2)
        }

        [PexGenericArguments(typeof(int), typeof(int), typeof(int))]
        [PexMethod]
        public DictionaryDictionary<TKey1, TKey2, TValue> Constructor<TKey1, TKey2, TValue>()
        {
            DictionaryDictionary<TKey1, TKey2, TValue> target = new DictionaryDictionary<TKey1, TKey2, TValue>()
              ;
            return target;
            // TODO: add assertions to method DictionaryDictionaryTKey1TKey2TValueTest.Constructor()
        }

        [PexGenericArguments(typeof(int), typeof(int), typeof(int))]
        [PexMethod]
        [PexAllowedException(typeof(KeyNotFoundException))]
        public TValue ItemGet<TKey1, TKey2, TValue>(
            [PexAssumeUnderTest]DictionaryDictionary<TKey1, TKey2, TValue> target,
            TKey1 key1,
            TKey2 key2
        )
        {
            TValue result = target[key1, key2];
            return result;
            // TODO: add assertions to method DictionaryDictionaryTKey1TKey2TValueTest.ItemGet(DictionaryDictionary`3<!!0,!!1,!!2>, !!0, !!1)
        }

        [PexGenericArguments(typeof(int), typeof(int), typeof(int))]
        [PexMethod]
        public bool TryGetValue<TKey1, TKey2, TValue>(
            [PexAssumeUnderTest]DictionaryDictionary<TKey1, TKey2, TValue> target,
            TKey1 key1,
            TKey2 key2,
            out TValue value
        )
        {
            bool result = target.TryGetValue(key1, key2, out value);
            return result;
            // TODO: add assertions to method DictionaryDictionaryTKey1TKey2TValueTest.TryGetValue(DictionaryDictionary`3<!!0,!!1,!!2>, !!0, !!1, !!2&)
        }

        [PexGenericArguments(typeof(int), typeof(int), typeof(int))]
        [PexMethod]
        public void ItemSet<TKey1, TKey2, TValue>(
            [PexAssumeUnderTest]DictionaryDictionary<TKey1, TKey2, TValue> target,
            TKey1 key1,
            TKey2 key2,
            TValue value
        )
        {
            target[key1, key2] = value;
            // TODO: add assertions to method DictionaryDictionaryTKey1TKey2TValueTest.ItemSet(DictionaryDictionary`3<!!0,!!1,!!2>, !!0, !!1, !!2)
        }
    }
}

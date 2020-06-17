using System;
using System.Collections.Generic;
using System.Text;
using Bsii.Dotnet.Utils.Collections;
using Xunit;

namespace Bsii.Dotnet.Utils.Tests
{
    public class DictionaryExtensionsTest
    {
        [Fact]
        public void TestBasicInsertion()
        {
            var dict = new Dictionary<int, int>();
            dict.AddOrUpdate(1, 1, 1);
            Assert.True(dict.ContainsKey(1));
        }

        [Fact]
        public void TestBasicUpdate()
        {
            var dict = new Dictionary<int, int> {[1] = 1};
            dict.AddOrUpdate(1, 1, 2);
            Assert.True(dict.ContainsKey(1));
            Assert.True(dict[1] == 2);
        }

        [Fact]
        public void TestBasicFactoryInsertion()
        {
            var dict = new Dictionary<int, int>();
            Func<int, int> addValueFactory = (key) => 1;
            Func<int, int, int> updateValueFactory = (key, value) => 2;
            dict.AddOrUpdate(1, addValueFactory, updateValueFactory);
            Assert.True(dict.ContainsKey(1));
            Assert.True(dict[1]==1);
        }

        [Fact]
        public void TestBasicFactoryUpdate()
        {
            var dict = new Dictionary<int, int> { [1] = 1 };
            Func<int, int> addValueFactory = (key) => 1;
            Func<int, int, int> updateValueFactory = (key, value) => 2;
            dict.AddOrUpdate(1, addValueFactory, updateValueFactory);
            Assert.True(dict.ContainsKey(1));
            Assert.True(dict[1] == 2);
        }
    }
}
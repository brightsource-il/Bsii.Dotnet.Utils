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
    }
}

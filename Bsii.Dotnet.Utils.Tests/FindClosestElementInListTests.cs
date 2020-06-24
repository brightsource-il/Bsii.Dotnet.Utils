using System.Collections.Generic;
using Xunit;
using Bsii.Dotnet.Utils.Collections;
using System;
using System.Linq;

namespace Bsii.Dotnet.Utils.Tests
{
    public class FindClosestElementInListTests
    {

        [Fact]
        public void TestFindClosestItemInListEmpty()
        {
            var l = new List<int>();
            Assert.Throws<ArgumentException>(() =>
            {
                var elem = l.FindClosestElement(7);
            });
        }

        private static readonly DateTime ReferencePoint = new DateTime(2020, 6, 24, 18, 20, 7);
        private static readonly List<DateTime> ReferenecTimeSeries = Enumerable.Range(0, 15).Select(i => ReferencePoint + TimeSpan.FromHours(i)).ToList();

        public static readonly IEnumerable<object[]> Inputs = new List<object[]>
        {
            new object[] { new List<int> { 1 }, 7, 1},
            new object[] { new List<double> { 1.2 }, 7.1, 1.2},
            new object[] { new List<int> { 1 }, -8, 1},
            new object[] { new List<int> { 1 }, 1, 1},
            new object[] { new List<int> { 1, 2, 3, 4, 5 }, 7, 5},
            new object[] { new List<double> { 1.2, 2.3, 3.4, 4.5, 5.6 }, 7.2, 5.6},
            new object[] { new List<double> { 1.2, 2.3, 3.4, 4.5, 5.6 }, 2.4, 2.3},
            new object[] { new List<int> { 1, 2, 3, 4, 5 }, -17, 1},
            new object[] { new List<int> { 1, 2, 3, 4, 5 }, 1, 1},
            new object[] { new List<int> { 1, 2, 3, 4, 5 }, 2, 2},
            new object[] { new List<int> { 1, 2, 3, 4, 5 }, 4, 4},
            new object[] { new List<int> { 1, 2, 3, 4, 5 }, 5, 5},
            new object[] { new List<int> { 10, 20, 30, 40 }, 11, 10},
            new object[] { new List<int> { 10, 20, 30, 40 }, 15, 10},
            new object[] { new List<int> { 10, 20, 30, 40 }, 17, 20},
            new object[] { new List<int> { 10, 20, 30, 40 }, 33, 30},
            new object[] { new List<int> { 10, 20, 30, 40 }, 38, 40},
            new object[] { ReferenecTimeSeries, ReferencePoint - TimeSpan.FromHours(1), ReferencePoint},
            new object[] { ReferenecTimeSeries, ReferencePoint + TimeSpan.FromMinutes(1), ReferencePoint},
            new object[] { ReferenecTimeSeries, ReferencePoint + TimeSpan.FromMinutes(45), ReferencePoint + TimeSpan.FromHours(1)},
            new object[] { ReferenecTimeSeries, ReferencePoint + TimeSpan.FromHours(8.2), ReferencePoint + TimeSpan.FromHours(8)},
            new object[] { ReferenecTimeSeries, ReferencePoint + TimeSpan.FromHours(8.8), ReferencePoint + TimeSpan.FromHours(9)},
            new object[] { ReferenecTimeSeries, ReferencePoint + TimeSpan.FromHours(17), ReferencePoint + TimeSpan.FromHours(14) },
        };

        [Theory]
        [MemberData(nameof(Inputs))]
        public void TestCorrectness<T>(List<T> inputs, T query, T result)
        {
            var res = inputs.FindClosestElement(query);
            Assert.Equal(result, res);
        }
    }
}

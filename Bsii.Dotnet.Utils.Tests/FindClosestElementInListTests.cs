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
            new object[] { new List<int> { 1 }, 7, 1, ComparisonOptions.LeftNeigbour},
            new object[] { new List<double> { 1.2 }, 7.1, 1.2, ComparisonOptions.RightNeighbour},
            new object[] { new List<int> { 1 }, -8, 1, ComparisonOptions.LeftNeigbour},
            new object[] { new List<int> { 1 }, 1, 1, ComparisonOptions.NearestNeighbour},
            new object[] { new List<int> { 1, 2, 3, 4, 5 }, 7, 5, ComparisonOptions.NearestNeighbour},
            new object[] { new List<double> { 1.2, 2.3, 3.4, 4.5, 5.6 }, 7.2, 5.6, ComparisonOptions.RightNeighbour},
            new object[] { new List<double> { 1.2, 2.3, 3.4, 4.5, 5.6 }, 2.4, 2.3, ComparisonOptions.NearestNeighbour},
            new object[] { new List<double> { 1.2, 2.3, 3.4, 4.5, 5.6 }, 3.3, 2.3, ComparisonOptions.LeftNeigbour},
            new object[] { new List<double> { 1.2, 2.3, 3.4, 4.5, 5.6 }, 2.4, 3.4, ComparisonOptions.RightNeighbour},
            new object[] { new List<int> { 1, 2, 3, 4, 5 }, -17, 1, ComparisonOptions.NearestNeighbour},
            new object[] { new List<int> { 1, 2, 3, 4, 5 }, 1, 1, ComparisonOptions.NearestNeighbour},
            new object[] { new List<int> { 1, 2, 3, 4, 5 }, 2, 2, ComparisonOptions.NearestNeighbour},
            new object[] { new List<int> { 1, 2, 3, 4, 5 }, 4, 4, ComparisonOptions.NearestNeighbour},
            new object[] { new List<int> { 1, 2, 3, 4, 5 }, 5, 5, ComparisonOptions.NearestNeighbour},
            new object[] { new List<int> { 10, 20, 30, 40 }, 11, 10, ComparisonOptions.NearestNeighbour},
            new object[] { new List<int> { 10, 20, 30, 40 }, 15, 10, ComparisonOptions.NearestNeighbour},
            new object[] { new List<int> { 10, 20, 30, 40 }, 17, 20, ComparisonOptions.NearestNeighbour},
            new object[] { new List<int> { 10, 20, 30, 40 }, 33, 30, ComparisonOptions.NearestNeighbour},
            new object[] { new List<int> { 10, 20, 30, 40 }, 38, 40, ComparisonOptions.NearestNeighbour},
            new object[] { ReferenecTimeSeries, ReferencePoint - TimeSpan.FromHours(1), ReferencePoint, ComparisonOptions.NearestNeighbour},
            new object[] { ReferenecTimeSeries, ReferencePoint + TimeSpan.FromMinutes(1), ReferencePoint, ComparisonOptions.NearestNeighbour},
            new object[] { ReferenecTimeSeries, ReferencePoint + TimeSpan.FromMinutes(45), ReferencePoint + TimeSpan.FromHours(1), ComparisonOptions.NearestNeighbour},
            new object[] { ReferenecTimeSeries, ReferencePoint + TimeSpan.FromHours(8.2), ReferencePoint + TimeSpan.FromHours(8), ComparisonOptions.NearestNeighbour},
            new object[] { ReferenecTimeSeries, ReferencePoint + TimeSpan.FromHours(8.8), ReferencePoint + TimeSpan.FromHours(9), ComparisonOptions.NearestNeighbour},
            new object[] { ReferenecTimeSeries, ReferencePoint + TimeSpan.FromHours(17), ReferencePoint + TimeSpan.FromHours(14), ComparisonOptions.NearestNeighbour },
        };

        [Theory]
        [MemberData(nameof(Inputs))]
        public void TestCorrectness<T>(List<T> inputs, T query, T result, ComparisonOptions cmpOpts)
        {
            var res = inputs.FindClosestElement(query, cmpOpts);
            Assert.Equal(result, res);
        }
    }
}

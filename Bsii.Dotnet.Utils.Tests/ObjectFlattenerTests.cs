using System;
using System.Collections.Generic;
using Bsii.Dotnet.Utils.Reflection;
using Xunit;

namespace Bsii.Dotnet.Utils.Tests
{
    public class ObjectFlattenerTests
    {
        [Fact]
        public void TestFlattening()
        {
            var myType = new
            {
                str = "hello",
                primitive = 3,
                enam = ConsoleSpecialKey.ControlBreak,
                array = new[] { 1, 2, 3 },
                dict = new Dictionary<string, double> { ["s"] = 1, ["k"] = 2.3 },
                dictComplex = new Dictionary<string, Vector3d>
                {
                    ["sx"] = new Vector3d(1, 2, 3),
                    ["kx"] = new Vector3d(4, 5, 6)
                },
                nested = new
                {
                    primitive2 = 1,
                    enam2 = ConsoleSpecialKey.ControlC,
                }
            };
            // Vector3d has computed properties, so trying to traverse them during flattening causes stack overflow
            // (which is guarded by the options.MaxDepth property and therfore we catch ArgumentOutOfRangeException) 
            Assert.Throws<ArgumentOutOfRangeException>(() => ObjectFlattener.FlattenToDictionary(myType));
            var res = ObjectFlattener.FlattenToDictionary(myType, new ObjectFlattener.Options
            {
                CustomConverters = new Dictionary<Type, Func<object, object>>
                {
                    [typeof(Vector3d)] = o =>
                    {
                        var vec = (Vector3d)o;
                        return new { vec.X, vec.Y, vec.Z };
                    }
                }
            });
            Assert.Equal(13, res.Count);
            Assert.DoesNotContain("array.0", res.Keys);
            Assert.Equal(4d, res["dictComplex.kx.X"]);
            Assert.Equal(6d, res["dictComplex.kx.Z"]);
            res = ObjectFlattener.FlattenToDictionary(myType, new ObjectFlattener.Options
            {
                CustomConverters = new Dictionary<Type, Func<object, object>> { [typeof(Vector3d)] = o => null },
                MaxElementsInEnumerable = 100,
                ShouldFlattenDictionaries = false,
                PropertyPathSeparator = '|'
            });
            Assert.Equal(8, res.Count);
            Assert.Contains("array|0", res.Keys);
            Assert.DoesNotContain("dictComplex|kx|X", res.Keys);
            Assert.Equal(3, res["array|2"]);

        }

        #region helper classes
        private class Vector3d
        {
            public double X { get; }
            public double Y { get; }
            public double Z { get; }

            /// <summary>
            /// To test object with recursive reference
            /// </summary>
            // ReSharper disable once MemberCanBePrivate.Local
            public Vector3d RefToSelf { get; }

            public Vector3d(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;

                RefToSelf = this;
            }
        }
        
        #endregion
    }
}

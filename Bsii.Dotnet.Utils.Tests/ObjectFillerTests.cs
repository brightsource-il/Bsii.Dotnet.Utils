using System;
using System.IO;
using Bsii.Dotnet.Utils.Reflection;
using Xunit;

namespace Bsii.Dotnet.Utils.Tests
{
    public class ObjectFillerTests
    {
        [Fact]
        public void TestFillingObject()
        {
            var props = new[] {"E.I.T.A.N", "E.T.H.A.N"};
            var value = new Root();
            ObjectFiller.FillObject(value, (path, type) =>
            {
                Assert.Equal(typeof(int), type);
                Assert.Contains(path, props);
                if (path == props[0])
                {
                    return 42;
                }
                return 84;
            }, props);
            Assert.Equal(42, value.E.I.T.A.N);
            Assert.Equal(84, value.E.T.H.A.N);
        }

        [Fact]
        public void TestFillingObjectFromCsv()
        {
            var csvText = "E.T.H.A.N,E.I.T.A.N\n84,42";
            using var reader = new CsvHelper.CsvReader(new StringReader(csvText));
            reader.Read();
            reader.ReadHeader();
            reader.Read();
            var value = new Root();
            // ReSharper disable once AccessToDisposedClosure
            ObjectFiller.FillObject(
                value, 
                (path, type) => reader.GetField(type, path), 
                reader.Context.HeaderRecord);
            Assert.Equal(42, value.E.I.T.A.N);
            Assert.Equal(84, value.E.T.H.A.N);

        }

        [Fact]
        public void TestFillingWrongObject()
        {
            var props = new[] { "M.I.C.H.A.E.L", "L.E.A.H.C.I.M" };
            var value = new Root();
            var ex = Assert.Throws<Exception>(()=>
                ObjectFiller.FillObject(value, (path, type) =>
                {
                    if (path == props[0])
                    {
                        return 42;
                    }
                    return 8.4;
                }, props));
            Assert.Contains("M.I.C.H.A.E.L", ex.Message);
        }

        #region Support classes


        class Root
        {
            public Eclass E { get; set; }
            
        }
        public class Eclass
        {
            public Iclass I { get; set; }
            public Tclass T { get; set; }

        }
        public class Iclass
        {
            public Tclass T { get; set; }

        }
        public class Tclass
        {
            public Aclass A { get; set; }
            public Hclass H { get; set; }
        }
        public class Hclass
        {
            public Aclass A { get; set; }
        }
        public class Aclass
        {
            public int N { get; set; }
        }


        #endregion
    }
}

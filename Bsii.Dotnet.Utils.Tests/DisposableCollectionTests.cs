using System;
using System.Collections.Generic;
using Bsii.Dotnet.Utils.Collections;
using Xunit;

namespace Bsii.Dotnet.Utils.Tests
{
    public class DisposableCollectionTests
    {
        [Fact]
        public void TestAsDisposable()
        {
            var disposedCount = 0;
            var disposableActionsList = new List<DisposableAction>
            {
                new DisposableAction(()=>disposedCount++),
                new DisposableAction(()=>disposedCount++),
                new DisposableAction(()=>disposedCount++)
            };
            disposableActionsList.AsDisposable().Dispose();
            Assert.Equal(3, disposedCount);
        }

        [Fact]
        public void TestAsDisposableWithOutParameter()
        {
            var disposedCount = 0;
            using (new List<DisposableAction>
            {
                new DisposableAction(() => disposedCount++),
                new DisposableAction(() => disposedCount++),
                new DisposableAction(() => disposedCount++)
            }.AsDisposable(out var disposableActionsList))
            {
                Assert.Equal(3, disposableActionsList.Count);
                Assert.Equal(0, disposedCount);
            }

            Assert.Equal(3, disposedCount);
        }

        [Fact]
        public void TestContinueOnException()
        {
            var disposedCount = 0;
            Assert.Throws<AggregateException>(() =>
            {
                using (new List<DisposableAction>
                {
                    new DisposableAction(() => disposedCount++),
                    new DisposableAction(() => throw new ArgumentException()),
                    new DisposableAction(() => disposedCount++)
                }.AsDisposable(out var disposableActionsList))
                {
                    Assert.Equal(3, disposableActionsList.Count);
                    Assert.Equal(0, disposedCount);
                }
            });
            Assert.Equal(2, disposedCount);
        }
    }
}

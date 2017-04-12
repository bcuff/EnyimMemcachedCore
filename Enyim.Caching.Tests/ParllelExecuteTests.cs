using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;

namespace Enyim.Caching.Tests
{
    public class ParllelExecuteTests
    {
        [Theory]
        [InlineData(1000, 10, 10)]
        [InlineData(10, 1000, 1000)]
        public void ParallelUtil_Execute_Should_Work(int firstDelay, int secondDelay, int thirdDelay)
        {
            var count = 0;
            ParallelUtil.Execute(
                () => { Thread.Sleep(firstDelay); Interlocked.Increment(ref count); },
                () => { Thread.Sleep(secondDelay); Interlocked.Increment(ref count); },
                () => { Thread.Sleep(thirdDelay); Interlocked.Increment(ref count); }
            );
            Assert.Equal(3, count);
        }

        [Fact]
        public void ParallelUtil_Execute_Should_Work_With_No_Actions()
        {
            ParallelUtil.Execute();
        }
    }
}

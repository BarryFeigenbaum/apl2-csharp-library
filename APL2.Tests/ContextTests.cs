using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using APL2.Types;
using Xunit;

namespace APL2.Tests
{
    public class ContextTests
    {
        [Fact]
        public void ContextCreation_UsesDefaults()
        {
            var context = new APLContext();

            Assert.Equal(0, context.IndexOrigin);
            Assert.Equal(80, context.PrintWidth);
            Assert.Equal(6, context.PrintPrecision);
            Assert.Equal(1e-15, context.ComparisonTolerance, 15);
            Assert.Equal(0, APLRuntime.Current.IndexOrigin);
        }

        [Fact]
        public void ContextPushPop_IsolatesChanges()
        {
            var original = APLRuntime.Current;
            var context = APLContext.Create();
            context.IndexOrigin = 1;

            APLContext.Push(context);
            try
            {
                Assert.NotSame(context, APLRuntime.Current);
                Assert.Equal(1, APLRuntime.Current.IndexOrigin);
                Assert.Equal(0, original.IndexOrigin);
            }
            finally
            {
                var popped = APLContext.Pop();
                Assert.Equal(context.IndexOrigin, popped.IndexOrigin);
                Assert.Equal(context.PrintWidth, popped.PrintWidth);
                Assert.Equal(context.PrintPrecision, popped.PrintPrecision);
                Assert.Equal(context.ComparisonTolerance, popped.ComparisonTolerance, 15);
            }

            Assert.Same(original, APLRuntime.Current);
            Assert.Equal(0, APLRuntime.Current.IndexOrigin);
        }

        [Fact]
        public void NestedContexts_InheritProperties()
        {
            var parent = APLContext.Create();
            parent.PrintPrecision = 3;

            APLContext.Push(parent);
            try
            {
                var child = APLContext.Create();

                Assert.Equal(3, child.PrintPrecision);

                child.IndexOrigin = 1;
                APLContext.Push(child);
                try
                {
                    Assert.Equal(1, APLRuntime.Current.IndexOrigin);
                    Assert.Equal(3, APLRuntime.Current.PrintPrecision);
                }
                finally
                {
                    APLContext.Pop();
                }

                Assert.Equal(0, APLRuntime.Current.IndexOrigin);
                Assert.Equal(3, APLRuntime.Current.PrintPrecision);
            }
            finally
            {
                APLContext.Pop();
            }
        }

        [Fact]
        public void IndexOrigin_AffectsArrayAccess()
        {
            var array = new ArrayType(new List<APLType>
            {
                new IntegerType(10),
                new IntegerType(20),
                new IntegerType(30)
            });

            Assert.Equal(20, ((IntegerType)array.GetElement(1)).Value);

            var context = APLContext.Create();
            context.IndexOrigin = 1;
            APLContext.Push(context);
            try
            {
                Assert.Equal(20, ((IntegerType)array.GetElement(2)).Value);
                Assert.Throws<IndexOutOfRangeException>(() => array.GetElement(0));
            }
            finally
            {
                APLContext.Pop();
            }
        }

        [Fact]
        public void PrintPrecision_AffectsFloatFormatting()
        {
            var value = new FloatingPointType(3.1415926535);
            var context = APLContext.Create();
            context.PrintPrecision = 3;

            APLContext.Push(context);
            try
            {
                Assert.Equal("3.14", value.ToString());
            }
            finally
            {
                APLContext.Pop();
            }
        }

        [Fact]
        public void BooleanFormatting_RemainsStable()
        {
            Assert.Equal("1", new BooleanType(true).ToString());
            Assert.Equal("0", new BooleanType(false).ToString());
        }

        [Fact]
        public void PrintWidth_AffectsArrayDisplayTruncation()
        {
            var array = new ArrayType(new List<APLType>
            {
                new IntegerType(1),
                new IntegerType(2),
                new IntegerType(3),
                new IntegerType(4),
                new IntegerType(5),
                new IntegerType(6)
            });

            var context = APLContext.Create();
            context.PrintWidth = 12;

            APLContext.Push(context);
            try
            {
                var display = array.ToString();
                Assert.EndsWith("...", display);
                Assert.True(display.Length <= 12);
            }
            finally
            {
                APLContext.Pop();
            }
        }

        [Fact]
        public void ComparisonTolerance_AffectsEqualityChecks()
        {
            var left = new FloatingPointType(1.0);
            var right = new FloatingPointType(1.0 + 5e-13);

            Assert.False(left.Equals(right));

            var context = APLContext.Create();
            context.ComparisonTolerance = 1e-12;

            APLContext.Push(context);
            try
            {
                Assert.True(left.Equals(right));
            }
            finally
            {
                APLContext.Pop();
            }
        }

        [Fact]
        public async Task Context_FlowsAcrossAsyncBoundaries()
        {
            var context = APLContext.Create();
            context.PrintPrecision = 4;

            APLContext.Push(context);
            try
            {
                Assert.Equal("3.142", await FormatAsync(new FloatingPointType(3.1415926535)));
            }
            finally
            {
                APLContext.Pop();
            }
        }

        [Fact]
        public void ComparisonTolerance_RemainsHashSetCompatible()
        {
            var values = new HashSet<APLType>();
            var context = APLContext.Create();
            context.ComparisonTolerance = 1e-12;

            APLContext.Push(context);
            try
            {
                Assert.True(values.Add(new IntegerType(1)));
                Assert.False(values.Add(new FloatingPointType(1.0 + 5e-13)));
            }
            finally
            {
                APLContext.Pop();
            }
        }

        private static async Task<string> FormatAsync(APLType value)
        {
            await Task.Yield();
            return value.ToString();
        }
    }
}

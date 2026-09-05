using System;
using System.Collections.Generic;
using System.Threading;

namespace APL2
{
    public sealed class APLContext
    {
        public int IndexOrigin { get; set; }
        public int PrintWidth { get; set; }
        public int PrintPrecision { get; set; }
        public double ComparisonTolerance { get; set; }

        public APLContext(
            int indexOrigin = 0,
            int printWidth = 80,
            int printPrecision = 6,
            double comparisonTolerance = 1e-15)
        {
            IndexOrigin = indexOrigin;
            PrintWidth = printWidth;
            PrintPrecision = printPrecision;
            ComparisonTolerance = comparisonTolerance;
        }

        public APLContext Clone() =>
            new APLContext(IndexOrigin, PrintWidth, PrintPrecision, ComparisonTolerance);

        public static APLContext Create() => APLRuntime.Current.Clone();

        public static void Push(APLContext context) => APLRuntime.Push(context);

        public static APLContext Pop() => APLRuntime.Pop();

        public static APLContext Current() => APLRuntime.Current;
    }

    public static class APLRuntime
    {
        private static readonly APLContext DefaultContext = new APLContext();
        private static readonly AsyncLocal<Stack<APLContext>> ContextStack = new AsyncLocal<Stack<APLContext>>();

        public static APLContext Current => GetStack().Peek();

        public static void Push(APLContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            GetStack().Push(context.Clone());
        }

        public static APLContext Pop()
        {
            var stack = GetStack();
            if (stack.Count == 1)
                throw new InvalidOperationException("Cannot pop the default APL context.");

            return stack.Pop();
        }

        private static Stack<APLContext> GetStack()
        {
            if (ContextStack.Value == null)
            {
                var stack = new Stack<APLContext>();
                stack.Push(DefaultContext.Clone());
                ContextStack.Value = stack;
            }

            return ContextStack.Value;
        }
    }
}

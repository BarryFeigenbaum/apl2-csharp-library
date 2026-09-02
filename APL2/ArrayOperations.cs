using System;
using System.Collections.Generic;
using System.Linq;
using APL2.Types;

namespace APL2.Operations
{
    public static class ArrayOperations
    {
        public static ArrayType Reshape(ArrayType array, params int[] newShape) =>
            array.Reshape(newShape);

        public static ArrayType Flatten(ArrayType array) => array.Flatten();

        public static ArrayType Reverse(ArrayType array)
        {
            var elements = new List<APLType>(array.Elements);
            elements.Reverse();
            return new ArrayType(elements, array.GetShape());
        }

        public static ArrayType Rotate(ArrayType array, int n)
        {
            var elements = array.Elements;
            int size = elements.Count;
            if (size == 0) return array;
            n = n % size;
            if (n < 0) n += size;
            var rotated = new List<APLType>(elements.Skip(size - n).Concat(elements.Take(size - n)));
            return new ArrayType(rotated, array.GetShape());
        }

        public static ArrayType Transpose(ArrayType array) => array.Transpose();

        public static ArrayType Concatenate(ArrayType left, ArrayType right)
        {
            var combined = new List<APLType>(left.Elements);
            combined.AddRange(right.Elements);
            var newShape = left.GetShape();
            newShape[0] = left.GetShape()[0] + right.GetShape()[0];
            return new ArrayType(combined, newShape);
        }

        public static ArrayType Take(ArrayType array, int n)
        {
            var elements = array.Elements.Take(n).ToList();
            if (n > array.Elements.Count)
                for (int i = array.Elements.Count; i < n; i++)
                    elements.Add(new IntegerType(0));
            var newShape = array.GetShape();
            newShape[0] = n;
            return new ArrayType(elements, newShape);
        }

        public static ArrayType Drop(ArrayType array, int n)
        {
            var dropped = array.Elements.Skip(Math.Min(n, array.Elements.Count)).ToList();
            var newShape = array.GetShape();
            newShape[0] = Math.Max(0, array.Elements.Count - n);
            return new ArrayType(dropped, newShape);
        }

        public static int Count(ArrayType array) =>
            array.Elements.Count(e => (e as Scalar)?.ToBoolean() ?? false);

        public static APLType Sum(ArrayType array)
        {
            if (array.Elements.Count == 0)
                return new IntegerType(0);
            APLType result = array.Elements[0].DeepCopy();
            for (int i = 1; i < array.Elements.Count; i++)
                result = MathOperations.Add(result, array.Elements[i]);
            return result;
        }

        public static APLType Product(ArrayType array)
        {
            if (array.Elements.Count == 0)
                return new IntegerType(1);
            APLType result = array.Elements[0].DeepCopy();
            for (int i = 1; i < array.Elements.Count; i++)
                result = MathOperations.Multiply(result, array.Elements[i]);
            return result;
        }

        public static APLType Maximum(ArrayType array)
        {
            if (array.Elements.Count == 0)
                throw new InvalidOperationException("Cannot find maximum of empty array");
            APLType result = array.Elements[0];
            for (int i = 1; i < array.Elements.Count; i++)
                result = MathOperations.Max(result, array.Elements[i]);
            return result;
        }

        public static APLType Minimum(ArrayType array)
        {
            if (array.Elements.Count == 0)
                throw new InvalidOperationException("Cannot find minimum of empty array");
            APLType result = array.Elements[0];
            for (int i = 1; i < array.Elements.Count; i++)
                result = MathOperations.Min(result, array.Elements[i]);
            return result;
        }
    }
}

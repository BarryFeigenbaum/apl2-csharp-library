using System;
using System.Collections.Generic;
using System.Linq;

namespace APL2.Types
{
    public abstract class APLType
    {
        public abstract string GetTypeName();
        public abstract APLType DeepCopy();
        public abstract int GetRank();
        public abstract int[] GetShape();
    }

    public abstract class Scalar : APLType
    {
        public override int GetRank() => 0;
        public override int[] GetShape() => Array.Empty<int>();

        public abstract double ToNumeric();
        public abstract bool ToBoolean();
        public abstract string ToCharacter();
    }

    public class BooleanType : Scalar
    {
        public bool Value { get; set; }

        public BooleanType(bool value)
        {
            Value = value;
        }

        public override string GetTypeName() => "Boolean";
        public override APLType DeepCopy() => new BooleanType(Value);
        public override double ToNumeric() => Value ? 1.0 : 0.0;
        public override bool ToBoolean() => Value;
        public override string ToCharacter() => Value ? "1" : "0";
    }

    public class IntegerType : Scalar
    {
        public int Value { get; set; }

        public IntegerType(int value)
        {
            Value = value;
        }

        public override string GetTypeName() => "Integer";
        public override APLType DeepCopy() => new IntegerType(Value);
        public override double ToNumeric() => Value;
        public override bool ToBoolean() => Value != 0;
        public override string ToCharacter() => ((char)Value).ToString();
    }

    public class FloatingPointType : Scalar
    {
        private const double EPSILON = 1e-15;
        public double Value { get; set; }

        public FloatingPointType(double value)
        {
            Value = value;
        }

        public override string GetTypeName() => "FloatingPoint";
        public override APLType DeepCopy() => new FloatingPointType(Value);
        public override double ToNumeric() => Value;
        public override bool ToBoolean() => Math.Abs(Value) > EPSILON;
        public override string ToCharacter() => ((char)(int)Value).ToString();
    }

    public class ComplexType : Scalar
    {
        private const double EPSILON = 1e-15;
        public double Real { get; set; }
        public double Imaginary { get; set; }

        public ComplexType(double real, double imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }

        public override string GetTypeName() => "Complex";
        public override APLType DeepCopy() => new ComplexType(Real, Imaginary);
        public override double ToNumeric() => Math.Sqrt(Real * Real + Imaginary * Imaginary);
        public override bool ToBoolean() => Math.Abs(Real) > EPSILON || Math.Abs(Imaginary) > EPSILON;
        public override string ToCharacter() => ((char)(int)Real).ToString();

        public ComplexType Add(ComplexType other) => 
            new ComplexType(Real + other.Real, Imaginary + other.Imaginary);

        public ComplexType Subtract(ComplexType other) => 
            new ComplexType(Real - other.Real, Imaginary - other.Imaginary);

        public ComplexType Multiply(ComplexType other) =>
            new ComplexType(
                Real * other.Real - Imaginary * other.Imaginary,
                Real * other.Imaginary + Imaginary * other.Real
            );

        public ComplexType Divide(ComplexType other)
        {
            double denom = other.Real * other.Real + other.Imaginary * other.Imaginary;
            return new ComplexType(
                (Real * other.Real + Imaginary * other.Imaginary) / denom,
                (Imaginary * other.Real - Real * other.Imaginary) / denom
            );
        }
    }

    public class StringType : Scalar
    {
        public string Value { get; set; }

        public StringType(string value)
        {
            Value = value ?? string.Empty;
        }

        public override string GetTypeName() => "String";
        public override APLType DeepCopy() => new StringType(Value);
        public override double ToNumeric() => double.TryParse(Value, out var result) ? result : 0.0;
        public override bool ToBoolean() => Value.Length > 0;
        public override string ToCharacter() => Value.Length > 0 ? Value[0].ToString() : "\0";
    }

    public class ArrayType : APLType
    {
        private const int MAX_DEPTH = 15;
        public List<APLType> Elements { get; set; }
        public int[] Shape { get; set; }
        public int Rank { get; private set; }

        public ArrayType(List<APLType> elements, int[] shape = null)
        {
            Elements = new List<APLType>(elements);
            if (shape != null)
            {
                Shape = (int[])shape.Clone();
                int expectedSize = shape.Aggregate(1, (a, b) => a * b);
                if (elements.Count != expectedSize)
                    throw new ArgumentException($"Elements size {elements.Count} doesn't match shape {string.Join(",", shape)}");
            }
            else
            {
                Shape = new[] { elements.Count };
            }
            Rank = Shape.Length;
        }

        public override string GetTypeName() => "Array";
        public override APLType DeepCopy() => 
            new ArrayType(Elements.Select(e => e.DeepCopy()).ToList(), Shape);
        public override int GetRank() => Rank;
        public override int[] GetShape() => (int[])Shape.Clone();

        public APLType GetElement(params int[] indices)
        {
            if (indices.Length == 1)
                return Elements[indices[0]];
            int flatIndex = ToFlatIndex(indices);
            return Elements[flatIndex];
        }

        private int ToFlatIndex(int[] indices)
        {
            int flatIndex = 0;
            int multiplier = 1;
            for (int i = Rank - 1; i >= 0; i--)
            {
                if (indices[i] < 0 || indices[i] >= Shape[i])
                    throw new IndexOutOfRangeException($"Index out of bounds");
                flatIndex += indices[i] * multiplier;
                multiplier *= Shape[i];
            }
            return flatIndex;
        }

        public ArrayType Reshape(params int[] newShape)
        {
            int newSize = newShape.Aggregate(1, (a, b) => a * b);
            if (newSize != Elements.Count)
                throw new ArgumentException($"Cannot reshape array of size {Elements.Count} to shape");
            return new ArrayType(Elements, newShape);
        }

        public ArrayType Flatten() => new ArrayType(Elements);

        public ArrayType Transpose()
        {
            if (Rank != 2)
                throw new InvalidOperationException("Transpose only works on 2-D arrays");
            int rows = Shape[0], cols = Shape[1];
            var transposed = new List<APLType>();
            for (int j = 0; j < cols; j++)
                for (int i = 0; i < rows; i++)
                    transposed.Add(Elements[i * cols + j]);
            return new ArrayType(transposed, new[] { cols, rows });
        }
    }
}

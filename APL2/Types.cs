using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using APL2;

namespace APL2.Types
{
    public abstract class APLType
    {
        public abstract string GetTypeName();
        public abstract APLType DeepCopy();
        public abstract int GetRank();
        public abstract int[] GetShape();
    }

    internal static class APLFormatting
    {
        public static string FormatNumber(double value) =>
            value.ToString($"G{Math.Max(1, APLRuntime.Current.PrintPrecision)}", CultureInfo.InvariantCulture);

        public static string ConstrainWidth(string value)
        {
            int printWidth = Math.Max(4, APLRuntime.Current.PrintWidth);
            if (value.Length <= printWidth)
                return value;

            return value.Substring(0, printWidth - 3) + "...";
        }
    }

    internal static class APLTypeComparer
    {
        public static bool AreEqual(APLType left, APLType right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (left == null || right == null)
                return false;

            if (left is ArrayType leftArray && right is ArrayType rightArray)
            {
                return leftArray.Shape.SequenceEqual(rightArray.Shape) &&
                       leftArray.Elements.Count == rightArray.Elements.Count &&
                       leftArray.Elements.Zip(rightArray.Elements, AreEqual).All(equal => equal);
            }

            if (TryGetComplexParts(left, out var leftReal, out var leftImaginary) &&
                TryGetComplexParts(right, out var rightReal, out var rightImaginary))
            {
                return NearlyEqual(leftReal, rightReal) && NearlyEqual(leftImaginary, rightImaginary);
            }

            if (left is BooleanType leftBoolean && right is BooleanType rightBoolean)
                return leftBoolean.Value == rightBoolean.Value;

            if (left is StringType leftString && right is StringType rightString)
                return leftString.Value == rightString.Value;

            return false;
        }

        public static int GetHashCode(APLType value)
        {
            if (value is BooleanType booleanValue)
                return booleanValue.Value.GetHashCode();
            if (value is StringType stringValue)
                return stringValue.Value.GetHashCode();
            return 0;
        }

        private static bool NearlyEqual(double left, double right) =>
            Math.Abs(left - right) <= APLRuntime.Current.ComparisonTolerance;

        private static bool TryGetComplexParts(APLType value, out double real, out double imaginary)
        {
            if (value is IntegerType integerValue)
            {
                real = integerValue.Value;
                imaginary = 0.0;
                return true;
            }

            if (value is FloatingPointType floatingPointValue)
            {
                real = floatingPointValue.Value;
                imaginary = 0.0;
                return true;
            }

            if (value is ComplexType complexValue)
            {
                real = complexValue.Real;
                imaginary = complexValue.Imaginary;
                return true;
            }

            real = 0.0;
            imaginary = 0.0;
            return false;
        }
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
        public override string ToString() => ToCharacter();
        public override bool Equals(object obj) => obj is APLType other && APLTypeComparer.AreEqual(this, other);
        public override int GetHashCode() => APLTypeComparer.GetHashCode(this);
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
        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        public override bool Equals(object obj) => obj is APLType other && APLTypeComparer.AreEqual(this, other);
        public override int GetHashCode() => APLTypeComparer.GetHashCode(this);
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
        public override string ToString() => APLFormatting.FormatNumber(Value);
        public override bool Equals(object obj) => obj is APLType other && APLTypeComparer.AreEqual(this, other);
        public override int GetHashCode() => APLTypeComparer.GetHashCode(this);
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
        public override string ToString() => $"{APLFormatting.FormatNumber(Real)}J{APLFormatting.FormatNumber(Imaginary)}";
        public override bool Equals(object obj) => obj is APLType other && APLTypeComparer.AreEqual(this, other);
        public override int GetHashCode() => APLTypeComparer.GetHashCode(this);

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
        public override string ToString() => Value;
        public override bool Equals(object obj) => obj is APLType other && APLTypeComparer.AreEqual(this, other);
        public override int GetHashCode() => APLTypeComparer.GetHashCode(this);
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
            if (Rank == 1 && indices.Length == 1)
            {
                int adjustedIndex = indices[0] - APLRuntime.Current.IndexOrigin;
                if (adjustedIndex < 0 || adjustedIndex >= Elements.Count)
                    throw new IndexOutOfRangeException("Index out of bounds");

                return Elements[adjustedIndex];
            }

            return Elements[ToFlatIndex(indices)];
        }

        private int ToFlatIndex(int[] indices)
        {
            if (indices.Length != Rank)
                throw new ArgumentException("Incorrect number of indices");

            int indexOrigin = APLRuntime.Current.IndexOrigin;
            int flatIndex = 0;
            int multiplier = 1;
            for (int i = Rank - 1; i >= 0; i--)
            {
                int adjustedIndex = indices[i] - indexOrigin;
                if (adjustedIndex < 0 || adjustedIndex >= Shape[i])
                    throw new IndexOutOfRangeException($"Index out of bounds");
                flatIndex += adjustedIndex * multiplier;
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

        public override string ToString()
        {
            int printWidth = Math.Max(4, APLRuntime.Current.PrintWidth);
            int maxVisibleWidth = printWidth - 3;
            var builder = new StringBuilder("[");

            for (int i = 0; i < Elements.Count; i++)
            {
                string separator = i == 0 ? string.Empty : " ";
                string elementText = Elements[i].ToString();

                if (builder.Length + separator.Length + elementText.Length > maxVisibleWidth)
                    return builder.Length == 1 ? APLFormatting.ConstrainWidth("[...]") : builder.ToString() + "...";

                builder.Append(separator);
                builder.Append(elementText);
            }

            if (builder.Length + 1 > printWidth)
                return APLFormatting.ConstrainWidth(builder.ToString() + "]");

            builder.Append(']');
            return builder.ToString();
        }

        public override bool Equals(object obj) => obj is APLType other && APLTypeComparer.AreEqual(this, other);

        public override int GetHashCode() => 0;
    }
}

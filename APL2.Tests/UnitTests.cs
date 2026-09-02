using System;
using System.Collections.Generic;
using Xunit;
using APL2.Types;
using APL2.Operations;

namespace APL2.Tests
{
    public class TypeTests
    {
        [Fact]
        public void BooleanType_Creation()
        {
            var trueVal = new BooleanType(true);
            var falseVal = new BooleanType(false);
            Assert.True(trueVal.Value);
            Assert.False(falseVal.Value);
            Assert.Equal(1.0, trueVal.ToNumeric());
            Assert.Equal(0.0, falseVal.ToNumeric());
        }

        [Fact]
        public void IntegerType_Creation()
        {
            var num = new IntegerType(42);
            Assert.Equal(42, num.Value);
            Assert.Equal(42.0, num.ToNumeric());
            Assert.True(num.ToBoolean());
        }

        [Fact]
        public void FloatingPointType_Creation()
        {
            var num = new FloatingPointType(3.14);
            Assert.Equal(3.14, num.Value, 2);
            Assert.True(num.ToBoolean());
        }

        [Fact]
        public void ComplexType_Creation()
        {
            var c = new ComplexType(3.0, 4.0);
            Assert.Equal(3.0, c.Real);
            Assert.Equal(4.0, c.Imaginary);
            Assert.Equal(5.0, c.ToNumeric());
        }
    }

    public class MathOperationsTests
    {
        [Fact]
        public void Add_Integers()
        {
            var result = MathOperations.Add(new IntegerType(5), new IntegerType(3)) as IntegerType;
            Assert.Equal(8, result.Value);
        }

        [Fact]
        public void Subtract_Integers()
        {
            var result = MathOperations.Subtract(new IntegerType(10), new IntegerType(3)) as IntegerType;
            Assert.Equal(7, result.Value);
        }

        [Fact]
        public void Multiply_Integers()
        {
            var result = MathOperations.Multiply(new IntegerType(6), new IntegerType(7)) as IntegerType;
            Assert.Equal(42, result.Value);
        }

        [Fact]
        public void Divide_Integers()
        {
            var result = MathOperations.Divide(new IntegerType(20), new IntegerType(4)) as FloatingPointType;
            Assert.Equal(5.0, result.ToNumeric(), 4);
        }

        [Fact]
        public void DivideByZero_Throws()
        {
            Assert.Throws<ArithmeticException>(() =>
                MathOperations.Divide(new IntegerType(5), new IntegerType(0))
            );
        }

        [Fact]
        public void Negate_Integer()
        {
            var result = MathOperations.Negate(new IntegerType(42)) as IntegerType;
            Assert.Equal(-42, result.Value);
        }

        [Fact]
        public void Abs_Integer()
        {
            var result = MathOperations.Abs(new IntegerType(-42)) as IntegerType;
            Assert.Equal(42, result.Value);
        }
    }

    public class ArrayOperationsTests
    {
        [Fact]
        public void CreateArray()
        {
            var elements = new List<APLType> { new IntegerType(1), new IntegerType(2), new IntegerType(3) };
            var array = new ArrayType(elements);
            Assert.Equal(1, array.GetRank());
            Assert.Equal(new[] { 3 }, array.GetShape());
        }

        [Fact]
        public void Sum_Array()
        {
            var elements = new List<APLType> { new IntegerType(1), new IntegerType(2), new IntegerType(3) };
            var array = new ArrayType(elements);
            var result = ArrayOperations.Sum(array) as IntegerType;
            Assert.Equal(6, result.Value);
        }

        [Fact]
        public void Product_Array()
        {
            var elements = new List<APLType> { new IntegerType(2), new IntegerType(3), new IntegerType(4) };
            var array = new ArrayType(elements);
            var result = ArrayOperations.Product(array) as IntegerType;
            Assert.Equal(24, result.Value);
        }

        [Fact]
        public void Reverse_Array()
        {
            var elements = new List<APLType> { new IntegerType(1), new IntegerType(2), new IntegerType(3) };
            var array = new ArrayType(elements);
            var reversed = ArrayOperations.Reverse(array);
            Assert.Equal(3, (reversed.Elements[0] as IntegerType).Value);
            Assert.Equal(1, (reversed.Elements[2] as IntegerType).Value);
        }
    }
}

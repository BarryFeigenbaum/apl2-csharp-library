using System;
using APL2.Types;

namespace APL2.Operations
{
    public static class MathOperations
    {
        public static APLType Add(APLType left, APLType right)
        {
            if (left is IntegerType intLeft && right is IntegerType intRight)
                return new IntegerType(intLeft.Value + intRight.Value);
            if (left is ComplexType compLeft && right is ComplexType compRight)
                return compLeft.Add(compRight);
            return new FloatingPointType(ToNumeric(left) + ToNumeric(right));
        }

        public static APLType Subtract(APLType left, APLType right)
        {
            if (left is IntegerType intLeft && right is IntegerType intRight)
                return new IntegerType(intLeft.Value - intRight.Value);
            if (left is ComplexType compLeft && right is ComplexType compRight)
                return compLeft.Subtract(compRight);
            return new FloatingPointType(ToNumeric(left) - ToNumeric(right));
        }

        public static APLType Multiply(APLType left, APLType right)
        {
            if (left is IntegerType intLeft && right is IntegerType intRight)
                return new IntegerType(intLeft.Value * intRight.Value);
            if (left is ComplexType compLeft && right is ComplexType compRight)
                return compLeft.Multiply(compRight);
            return new FloatingPointType(ToNumeric(left) * ToNumeric(right));
        }

        public static APLType Divide(APLType left, APLType right)
        {
            double rval = ToNumeric(right);
            if (Math.Abs(rval) < 1e-15)
                throw new ArithmeticException("Division by zero");
            if (left is IntegerType intLeft && right is IntegerType intRight)
                return new FloatingPointType((double)intLeft.Value / intRight.Value);
            if (left is ComplexType compLeft && right is ComplexType compRight)
                return compLeft.Divide(compRight);
            return new FloatingPointType(ToNumeric(left) / rval);
        }

        public static APLType Power(APLType left, APLType right)
        {
            double baseVal = ToNumeric(left);
            double expVal = ToNumeric(right);
            return new FloatingPointType(Math.Pow(baseVal, expVal));
        }

        public static APLType Negate(APLType operand)
        {
            if (operand is IntegerType intVal)
                return new IntegerType(-intVal.Value);
            if (operand is ComplexType compVal)
                return new ComplexType(-compVal.Real, -compVal.Imaginary);
            return new FloatingPointType(-ToNumeric(operand));
        }

        public static APLType Abs(APLType operand)
        {
            if (operand is IntegerType intVal)
                return new IntegerType(Math.Abs(intVal.Value));
            if (operand is ComplexType compVal)
                return new FloatingPointType(compVal.ToNumeric());
            return new FloatingPointType(Math.Abs(ToNumeric(operand)));
        }

        public static APLType Sqrt(APLType operand)
        {
            double value = ToNumeric(operand);
            if (value < 0)
                return new ComplexType(0, Math.Sqrt(-value));
            return new FloatingPointType(Math.Sqrt(value));
        }

        public static APLType Ceiling(APLType operand) =>
            operand is IntegerType ? operand : new IntegerType((int)Math.Ceiling(ToNumeric(operand)));

        public static APLType Floor(APLType operand) =>
            operand is IntegerType ? operand : new IntegerType((int)Math.Floor(ToNumeric(operand)));

        public static APLType Sign(APLType operand)
        {
            double value = ToNumeric(operand);
            if (value > 0) return new IntegerType(1);
            if (value < 0) return new IntegerType(-1);
            return new IntegerType(0);
        }

        public static APLType Log(APLType operand)
        {
            double value = ToNumeric(operand);
            if (value <= 0)
                throw new ArithmeticException("Logarithm of non-positive number");
            return new FloatingPointType(Math.Log(value));
        }

        public static APLType Exp(APLType operand) => 
            new FloatingPointType(Math.Exp(ToNumeric(operand)));

        public static APLType Sin(APLType operand) => 
            new FloatingPointType(Math.Sin(ToNumeric(operand)));

        public static APLType Cos(APLType operand) => 
            new FloatingPointType(Math.Cos(ToNumeric(operand)));

        public static APLType Tan(APLType operand) => 
            new FloatingPointType(Math.Tan(ToNumeric(operand)));

        public static APLType Max(APLType left, APLType right) =>
            new FloatingPointType(Math.Max(ToNumeric(left), ToNumeric(right)));

        public static APLType Min(APLType left, APLType right) =>
            new FloatingPointType(Math.Min(ToNumeric(left), ToNumeric(right)));

        private static double ToNumeric(APLType value)
        {
            if (value is Scalar scalar)
                return scalar.ToNumeric();
            throw new InvalidOperationException("Cannot convert non-scalar to numeric");
        }
    }
}

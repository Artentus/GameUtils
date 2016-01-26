namespace GameUtils.Math
{
    public static class MathHelper
    {
        public const float Pi = (float)System.Math.PI;
        public const float E = (float)System.Math.E;
        public const float Epsilon = 5.960464E-8f;

        public static float Sin(float value)
        {
            return (float)System.Math.Sin(value);
        }

        public static float Cos(float value)
        {
            return (float)System.Math.Cos(value);
        }

        public static float Tan(float value)
        {
            return (float)System.Math.Tan(value);
        }

        public static float Asin(float value)
        {
            return (float)System.Math.Asin(value);
        }

        public static float Acos(float value)
        {
            return (float)System.Math.Acos(value);
        }

        public static float Atan(float value)
        {
            return (float)System.Math.Atan(value);
        }

        public static float Atan2(float y, float x)
        {
            return (float)System.Math.Atan2(y, x);
        }

        public static float Log(float value)
        {
            return (float)System.Math.Log(value);
        }

        public static float Log(float value, float @base)
        {
            return (float)System.Math.Log(value, @base);
        }

        public static float Log10(float value)
        {
            return (float)System.Math.Log10(value);
        }

        public static float Ceiling(float value)
        {
            return (float)System.Math.Ceiling(value);
        }

        public static float Floor(float value)
        {
            return (float)System.Math.Floor(value);
        }

        public static float Sqrt(float value)
        {
            return (float)System.Math.Sqrt(value);
        }

        public static float Pow(float @base, float exponent)
        {
            return (float)System.Math.Pow(@base, exponent);
        }

        public static float Pow(float @base, int exponent)
        {
            if (@base == 0)
            {
                if (exponent == 0)
                    return float.NaN;

                return 0;
            }

            if (@base == 1)
                return 1;

            if (exponent < 0)
                return 1 / Pow(@base, -exponent);

            float result = 1;
            while (exponent > 0)
            {
                if ((exponent & 1) != 0)
                    result *= @base;

                exponent >>= 1;
                @base *= @base;
            }
            return result;
        }

        public static float Max(params float[] values)
        {
            float max = float.MinValue;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > max)
                    max = values[i];
            }
            return max;
        }

        public static float Max(params int[] values)
        {
            int max = int.MinValue;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > max)
                    max = values[i];
            }
            return max;
        }

        public static float Min(params float[] values)
        {
            float min = float.MaxValue;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] < min)
                    min = values[i];
            }
            return min;
        }

        public static float Min(params int[] values)
        {
            int min = int.MaxValue;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] < min)
                    min = values[i];
            }
            return min;
        }

        public static void MinMax(out float min, out float max, params float[] values)
        {
            min = float.MaxValue;
            max = float.MinValue;
            for (int i = 0; i < values.Length; i++)
            {
                float value = values[i];

                if (value < min)
                    min = value;

                if (value > max)
                    max = value;
            }
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static int DivRem(float dividend, float divisor, out float remainder)
        {
            int result = (int)(dividend / divisor);
            remainder = dividend - result;
            return result;
        }

        public static int DivRem(double dividend, double divisor, out double remainder)
        {
            int result = (int)(dividend / divisor);
            remainder = dividend - result;
            return result;
        }

        public static int Factorial(int value)
        {
            int result = 1;
            for (int i = 2; i <= value; i++)
                result *= i;
            return result;
        }

        public static long BigFactorial(int value)
        {
            long result = 1;
            for (int i = 2; i <= value; i++)
                result *= i;
            return result;
        }
    }
}

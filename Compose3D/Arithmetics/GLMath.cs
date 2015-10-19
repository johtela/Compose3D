namespace Compose3D.Arithmetics
{
    using GLTypes;
	using OpenTK;
	using System;

    public static class GLMath
    {
        [GLFunction ("clamp ({0})")]
        public static float Clamp (this float value, float min, float max)
        {
            return Math.Min (Math.Max (value, min), max);
        }

        [GLFunction ("clamp ({0})")]
        public static double Clamp (this double value, double min, double max)
        {
            return Math.Min (Math.Max (value, min), max);
        }

        [GLFunction ("clamp ({0})")]
        public static int Clamp (this int value, int min, int max)
        {
            return Math.Min (Math.Max (value, min), max);
        }

        [GLFunction ("pow ({0})")]
        public static float Pow (this float value, float exponent)
        {
            return (float)Math.Pow (value, exponent);
        }

        [GLFunction ("pow ({0})")]
        public static double Pow (this double value, double exponent)
        {
            return Math.Pow (value, exponent);
        }

        [GLFunction ("pow ({0})")]
        public static int Pow (this int value, int exponent)
        {
            return (int)Math.Pow (value, exponent);
        }

        [GLFunction ("mix ({0})")]
        public static float Mix (float start, float end, float interPos)
        {
            return start * (1 - interPos) + end * interPos;
        }

        [GLFunction ("mix ({0})")]
        public static double Mix (double start, double end, double interPos)
        {
            return start * (1 - interPos) + end * interPos;
        }

        [GLFunction ("radians ({0})")]
        public static float Radians (this float degrees)
        {
            return degrees * MathHelper.Pi / 180f;
        }

        [GLFunction ("radians ({0})")]
        public static double Radians (this double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        [GLFunction ("radians ({0})")]
        public static Vec2 Radians (this Vec2 degrees)
        {
            return degrees * MathHelper.Pi / 180f;
        }

        [GLFunction ("radians ({0})")]
        public static Vec3 Radians (this Vec3 degrees)
        {
            return degrees * MathHelper.Pi / 180f;
        }

        [GLFunction ("radians ({0})")]
        public static Vec4 Radians (this Vec4 degrees)
        {
            return degrees * MathHelper.Pi / 180f;
        }

        [GLFunction ("radians ({0})")]
        public static float Degrees (this float radians)
        {
            return radians * 180f / MathHelper.Pi;
        }

        [GLFunction ("radians ({0})")]
        public static double Degrees (this double radians)
        {
            return radians * 180f / MathHelper.Pi;
        }

        [GLFunction ("radians ({0})")]
        public static Vec2 Degrees (this Vec2 radians)
        {
            return radians * 180f / MathHelper.Pi;
        }

        [GLFunction ("radians ({0})")]
        public static Vec3 Degrees (this Vec3 radians)
        {
            return radians * 180f / MathHelper.Pi;
        }

        [GLFunction ("radians ({0})")]
        public static Vec4 Degrees (this Vec4 radians)
        {
            return radians * 180f / MathHelper.Pi;
        }
    }
}

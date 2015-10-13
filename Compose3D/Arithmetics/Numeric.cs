namespace Compose3D.Arithmetics
{
    using GLTypes;
	using OpenTK;
	using System;

    public static class Numeric
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

        public static float ToRadians (this float degrees)
		{
			return degrees * MathHelper.Pi / 180f;
		}

		public static float ToRadians (this int degrees)
		{
			return (float)degrees * MathHelper.Pi / 180f;
		}

		public static double ToRadians (this double degrees)
		{
			return degrees * Math.PI / 180.0;
		}
    }
}

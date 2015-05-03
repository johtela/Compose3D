namespace Compose3D.Arithmetics
{
    using System;
    using GLTypes;

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
    }
}

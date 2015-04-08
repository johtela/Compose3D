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
    }
}

namespace ComposeFX.Maths
{
	using System;

	public class BSpline<V> where V : struct, IVec<V, float>
	{
		public readonly int PolynomialDegree;
		public readonly V[] ControlPoints;
		public readonly float[] Knots;

		public BSpline (int degree, V[] controlPoints, float[] knots)
		{
			PolynomialDegree = degree;
			ControlPoints = controlPoints;
			Knots = knots;
		}

		public float Basis (int degree, int index, float value)
		{
			if (degree == 0)
				return Knots [index] <= value && value < Knots [index + 1] ? 1f : 0f;
			else
			{
				var ki0 = Knots [index];
				var ki1 = Knots [index + 1];
				var kid0 = Knots [index + degree];
				var kid1 = Knots [index + degree + 1];
				return (kid0 == ki0 ? 0f : ((value - ki0) / (kid0 - ki0)) * Basis (degree - 1, index, value)) +
					(kid1 == ki1 ? 0f : ((kid1 - value) / (kid1 - ki1)) * Basis (degree - 1, index + 1, value));
			}
		}

		public V Evaluate (float value)
		{
			var result = ControlPoints [0].Multiply (Basis (PolynomialDegree, 0, value));
			for (int i = 1; i < ControlPoints.Length; i++)
				result = result.Add (ControlPoints [i].Multiply (Basis (PolynomialDegree, i, value)));
			return result;
		}

		public static BSpline<V> FromControlPoints (int degree, params V[] controlPoints)
		{
			var len = controlPoints.Length;
			if (len < 2)
				throw new ArgumentException ("You must give at least two control points.", "controlPoints");
            var knots = new float[len + degree + 1];
			var start = (degree / 2) + 1;
			var curr = 0;
			while (curr < start)
				knots [curr++] = 1f;
			while (curr < knots.Length)
				knots [curr++] = Math.Min (curr - start, len - 1);
			return new BSpline<V> (degree, controlPoints, knots);
		}
	}
}


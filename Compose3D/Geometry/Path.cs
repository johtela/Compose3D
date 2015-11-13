namespace Compose3D.Geometry
{
	using System;
	using System.Linq;
	using Maths;

	public class Path<P, V> 
		where P : struct, IPositional<V>
		where V : struct, IVec<V, float>
	{
		public P[] Vertices { get; private set; }

		public Path (P[] vertices)
		{
			Vertices = vertices;
		}

		public static Path<P, V> FromBSpline (BSpline<V> spline, int numVertices)
		{
			var vertices = new P[numVertices];
			var curr = spline.Knots.First ();
			var last = spline.Knots.Last () - 0.000001f;
			var step = (float)(last - curr) / (numVertices - 1);

			for (int i = 0; i < numVertices; i++)
			{
				vertices[i].Position = spline.Evaluate (curr);
				curr = Math.Min (curr + step, last);
			}
			return new Path<P, V> (vertices);
		}
	}
}

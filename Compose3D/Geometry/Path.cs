namespace Compose3D.Geometry
{
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
			var step = (float)(spline.Knots.Last () - curr) / numVertices;

			for (int i = 0; i < numVertices; i++)
			{
				vertices[i].Position = spline.Evaluate (curr);
				curr += step;
			}
			return new Path<P, V> (vertices);
		}
	}
}

namespace Compose3D.Geometry
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Maths;

	public class Path<P, V> 
		where P : struct, IPositional<V>
		where V : struct, IVec<V, float>
	{
		public P[] Nodes { get; private set; }

		public Path (IEnumerable<P> nodes)
		{
			var uniqNodes = nodes.RemoveConsequtiveDuplicates ().ToArray ();
			if (uniqNodes.Length < 2)
				throw new ArgumentException (
					@"Path must contain at least two vertices.
					Consequtive duplicate vertices are removed automatically.", 
					"vertices");
			Nodes = uniqNodes;
		}

		public static Path<P, V> FromBSpline (BSpline<V> spline, int numNodes)
		{
			var nodes = new P[numNodes];
			var curr = spline.Knots.First ();
			var last = spline.Knots.Last () - 0.000001f;
			var step = (float)(last - curr) / (numNodes - 1);

			for (int i = 0; i < numNodes; i++)
			{
				nodes[i].Position = spline.Evaluate (curr);
				curr = Math.Min (curr + step, last);
			}
			return new Path<P, V> (nodes);
		}

		public static Path<P, V> operator + (Path<P, V> path1, Path<P, V> path2)
		{
			return new Path<P, V> (path1.Nodes.Concat (path2.Nodes));
		}

		public static Path<P, V> operator + (Path<P, V> path1, P node)
		{
			return new Path<P, V> (path1.Nodes.Concat (new P[] { node }));
		}

		public Path<P, V> Close ()
		{
			return new Path<P, V> (Nodes.Concat (new P[] { Nodes[0] }));
		}
	}
}

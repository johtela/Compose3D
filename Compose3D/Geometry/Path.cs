namespace Compose3D.Geometry
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Maths;

	public class Path<P, V> : ITransformable<Path<P, V>, Mat4>
		where P : struct, IPositional<V>
		where V : struct, IVec<V, float>
	{
		public P[] Nodes { get; private set; }

		public Path (IEnumerable<P> nodes)
		{
			var uniqNodes = nodes.RemoveConsequtiveDuplicates ().ToArray ();
			if (uniqNodes.Length < 2)
				throw new ArgumentException (
					"Path must contain at least two vertices. Consequtive duplicate vertices " + 
					"are removed automatically.", "vertices");
			Nodes = uniqNodes;
		}

		public Path<P, V> Close ()
		{
			return new Path<P, V> (Nodes.Concat (new P[] { Nodes[0] }));
		}

		public bool IsClosed
		{
			get { return Nodes.First ().Position.Equals (Nodes.Last ().Position); }
		}

		public static Path<P, V> FromVecs (IEnumerable<V> positions)
		{
			return new Path<P, V> (positions.Select (p => new P () { Position = p }));
		}

		public static Path<P, V> FromVecs (params V[] positions)
		{
			return FromVecs ((IEnumerable<V>)positions);
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
		
		public Path<P, V> Transform (Mat4 matrix)
		{
			return new Path<P, V> (Nodes.Select (n => WithPosition (n, matrix.Transform (n.Position))));
		}
		
		public Path<P, V> ReverseWinding ()
		{
			return new Path<P, V> (Nodes.Reverse ());
		}
		
		public Path<P, V> RenumberNodes (int first)
		{
			if (!IsClosed)
				throw new ArgumentException ("Paths must be closed in order to renumber its nodes");
			return new Path<P, V> (Nodes.Slice (first, Nodes.Length - first - 1)
				.Concat (Nodes.Slice (0, first + 1)));
		}

		private void CheckSameLengthWith (Path<P, V> other)
		{
			if (other.Nodes.Length != Nodes.Length)
				throw new ArgumentException ("Paths must have same number of nodes", "other");
		}
		
		private P WithPosition (P node, V position)
		{
			node.Position = position;
			return node;
		}

		public Path<P, V> MorphWith (Path<P, V> other, float interPos)
		{
			CheckSameLengthWith (other);
			return new Path<P, V> (Nodes.Zip (other.Nodes, 
				(n1, n2) => WithPosition (n1, n1.Position.Mix (n2.Position, interPos))));
		}

		public static Path<P, V> operator + (Path<P, V> path1, Path<P, V> path2)
		{
			return new Path<P, V> (path1.Nodes.Concat (path2.Nodes));
		}

		public static Path<P, V> operator + (Path<P, V> path1, P node)
		{
			return new Path<P, V> (path1.Nodes.Concat (new P[] { node }));
		}
	}
}
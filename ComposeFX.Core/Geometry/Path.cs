namespace ComposeFX.Geometry
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Maths;
	using Extensions;

	public class Path<V, D> : ITransformable<Path<V, D>, Mat4>
		where V : struct, IVertex<D>
		where D : struct, IVec<D, float>
	{
		public V[] Vertices { get; private set; }

		public Path (IEnumerable<V> vertices)
		{
			var uniqVerts = vertices.RemoveConsequtiveDuplicates ().ToArray ();
			if (uniqVerts.Length < 2)
				throw new ArgumentException (
					"Path must contain at least two vertices. Consequtive duplicate vertices " + 
					"are removed automatically.", "vertices");
			Vertices = uniqVerts;
		}

		public Path<V, D> Close ()
		{
			return new Path<V, D> (Vertices.Concat (new V[] { Vertices[0] }));
		}

		public Path<V, D> Open ()
		{
			return IsClosed ? new Path<V, D> (Vertices.Take (Vertices.Length - 1)) : this;
		}

		public bool IsClosed
		{
			get { return Vertices.First ().position.Equals (Vertices.Last ().position); }
		}

		public static Path<V, D> FromVecs (IEnumerable<D> positions)
		{
			return new Path<V, D> (positions.Select (p => new V () { position = p }));
		}

		public static Path<V, D> FromVecs (params D[] positions)
		{
			return FromVecs ((IEnumerable<D>)positions);
		}
		
		public static Path<V, D> FromBSpline (BSpline<D> spline, int numNodes)
		{
			var nodes = new V[numNodes];
			var curr = spline.Knots.First ();
			var last = spline.Knots.Last () - 0.000001f;
			var step = (float)(last - curr) / (numNodes - 1);

			for (int i = 0; i < numNodes; i++)
			{
				nodes[i].position = spline.Evaluate (curr);
				curr = Math.Min (curr + step, last);
			}
			return new Path<V, D> (nodes);
		}
		
		public static Path<V, D> FromPie (float radiusX, float radiusY, float startAngle, float endAngle,
			int nodeCount)
		{
			if (radiusY <= 0f || radiusY <= 0f)
				throw new ArgumentException ("Radiuses have to be greater than zero.");
			if (startAngle == endAngle)
				endAngle += FMath.TwoPi;
			var stepAngle = (endAngle - startAngle) / (nodeCount - 1);
			var nodes = new V[nodeCount];
			var angle = startAngle;
			for (var i = 0; i < nodeCount; i++)
			{
				var pos = Vec.FromArray<D, float> (
					radiusX * (float)Math.Cos (angle), radiusY * (float)Math.Sin (angle), 0f, 0f);
				nodes [i] = new V() { position = pos };
				angle = angle + stepAngle;
			}
			return new Path<V, D> (nodes);
		}

		public static Path<V, D> FromRectangle (float width, float height)
		{
			var halfX = width / 2f;
			var halfY = height / 2f;
			return FromVecs (
				Vec.FromArray<D, float> (-halfX, -halfY),
				Vec.FromArray<D, float> (-halfX, halfY),
				Vec.FromArray<D, float> (halfX, halfY),
				Vec.FromArray<D, float> (halfX, -halfY)).Close ();
		}

		public Path<V, D> Transform (Mat4 matrix)
		{
			return new Path<V, D> (Vertices.Select (n => WithPosition (n, matrix.Transform (n.position))));
		}
		
		public Path<V, D> ReverseWinding ()
		{
			return new Path<V, D> (Vertices.Reverse ());
		}
		
		public Path<V, D> RenumberNodes (int first)
		{
			if (!IsClosed)
				throw new ArgumentException ("Paths must be closed in order to renumber its nodes");
			return new Path<V, D> (Vertices.Slice (first, Vertices.Length - first - 1)
				.Concat (Vertices.Slice (0, first + 1)));
		}

		private void CheckSameLengthWith (Path<V, D> other)
		{
			if (other.Vertices.Length != Vertices.Length)
				throw new ArgumentException ("Paths must have same number of nodes", "other");
		}
		
		private V WithPosition (V node, D position)
		{
			node.position = position;
			return node;
		}

		public Path<V, D> MorphWith (Path<V, D> other, float interPos)
		{
			CheckSameLengthWith (other);
			return new Path<V, D> (Vertices.Zip (other.Vertices, 
				(n1, n2) => WithPosition (n1, n1.position.Mix (n2.position, interPos))));
		}
		
		public IEnumerable<Path<V, D>> MorphTo (Path<V, D> other, IEnumerable<Mat4> transforms)
		{
			var step = 1f / transforms.Count ();
			var curr = 0f;
			
			foreach (var transform in transforms)
			{
				yield return MorphWith (other, curr).Transform (transform);
				curr += step;
			}
		}

		private IEnumerable<V> SubdividedNodes (int numDivisions)
		{
			for (int i = 0; i < Vertices.Length - 1; i++)
			{
				var current = Vertices[i];
				yield return current;

				var step = 1f / (numDivisions + 1f);
				for (int j = 1; j <= numDivisions; j++)
				{
					var pos = current.position.Mix (Vertices[i + 1].position, j * step);
					yield return WithPosition (current, pos);
				}
			}
			yield return Vertices[Vertices.Length - 1];
		}

		public Path<V, D> Subdivide (int numDivisions)
		{
			return new Path<V, D> (SubdividedNodes (numDivisions));
		}

		public static Path<V, D> operator + (Path<V, D> path1, Path<V, D> path2)
		{
			return new Path<V, D> (path1.Vertices.Concat (path2.Vertices));
		}

		public static Path<V, D> operator + (Path<V, D> path1, V node)
		{
			return new Path<V, D> (path1.Vertices.Concat (new V[] { node }));
		}

		public static Path<V, D> operator + (Path<V, D> path)
		{
			return path.Close ();
		}

		public static Path<V, D> operator - (Path<V, D> path)
		{
			return path.Open ();
		}
	}
}
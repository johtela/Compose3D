namespace Compose3D.Geometry
{
	using Maths;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Extrusion
	{
		#region Edges

		private class Edge : IEquatable<Edge>
		{
			public readonly int Index1;
			public readonly	int Index2;

			public Edge (int index1, int index2)
			{
				Index1 = index1;
				Index2 = index2;
			}

			public override bool Equals (object obj)
			{
				return obj is Edge && Equals ((Edge)obj);
			}

			public override int GetHashCode ()
			{
				return Index1.GetHashCode () ^ Index2.GetHashCode ();
			}

			public override string ToString ()
			{
				return string.Format ("({0}, {1})", Index1, Index2);
			}

			public bool Equals (Edge other)
			{
				return (Index1 == other.Index1 && Index2 == other.Index2) ||
					(Index1 == other.Index2 && Index2 == other.Index1);
			}
		}

		#endregion

		#region Private helper functions

		private static int BoolToInt (bool b)
		{
			return b ? 1 : 0;
		}

		private static V SideVertex<V> (V vertex, Vec3 normal)
			where V : struct, IVertex
		{
			return VertexHelpers.New<V> (vertex.Position, normal);
		}

		private static IEnumerable<Edge> GetEdges<V> (Geometry<V> geometry)
			where V : struct, IVertex
		{
			var indices = geometry.Indices;
			for (int i = 0; i < indices.Length; i += 3)
			{
				yield return new Edge (indices [i], indices [i + 1]);
				yield return new Edge (indices [i + 1], indices [i + 2]);
				yield return new Edge (indices [i + 2], indices [i]);
			}
		}
		
		private static IEnumerable<Edge> GetEdges<P> (Path<P, Vec3> path)
			where P : struct, IPositional<Vec3>
		{
			return Enumerable.Range (1, path.Nodes.Length - 1).Select (i => new Edge (i - 1, i));
		}

		private static IEnumerable<Edge> DetermineOuterEdges (Edge[] edges)
		{
			var innerEdges = new HashSet<Edge> ();

			for (int i = 0; i < edges.Length; i++)
				for (int j = i + 1; j < edges.Length; j++)
					if (edges [i].Equals (edges [j]))
					{
						innerEdges.Add (edges [j]);
						break;
					}
			return edges.Except (innerEdges);
		}

		private static Vec3 CalculateNormal<V> (V[] vertices, V[] backVertices, int index1, int index2) 
			where V : struct, IVertex
		{
			return vertices [index1].Position.CalculateNormal (
				vertices [index2].Position, backVertices [index1].Position);
		}

		private static V[] GetVertices<V, P> (Path<P, Vec3> path, bool flipNormal)
			where V : struct, IVertex
			where P : struct, IPositional<Vec3>
		{
			var nodes = path.Nodes;
			var normal = nodes [1].Position.CalculateNormal (nodes [0].Position, nodes [2].Position);
			if (flipNormal)
				normal = -normal;
			return nodes.Select (n => VertexHelpers.New<V> (n.Position, normal)).ToArray ();
		}

		private static int ExtrudeOut<V> (Geometry<V>[] geometries, int i, V[] vertices, V[] backVertices,
			Edge[] outerEdges) where V : struct, IVertex
		{
			for (var j = 0; j < outerEdges.Length; j++)
			{
				var edge = outerEdges[j];
				var frontNormal = CalculateNormal (vertices, backVertices, edge.Index1, edge.Index2);
				var backNormal = CalculateNormal (backVertices, vertices, edge.Index2, edge.Index1);
				geometries[i++] = Quadrilateral<V>.FromVertices (
					SideVertex (vertices[edge.Index2], frontNormal),
					SideVertex (vertices[edge.Index1], frontNormal),
					SideVertex (backVertices[edge.Index1], backNormal),
					SideVertex (backVertices[edge.Index2], backNormal));
			}
			return i;
		}

		#endregion

		public static Geometry<V> Stretch<V> (this Geometry<V> frontFace, IEnumerable<Mat4> transforms,
			bool includeFrontFace, bool includeBackFace) 
			where V : struct, IVertex
		{
			var vertices = frontFace.Vertices;
			if (!vertices.AreCoplanar ())
				throw new ArgumentException ("Geometry is not on completely on the same plane.", "frontFace");
			var firstNormal = vertices[0].Normal;
			if (!vertices.All (v => Vec.ApproxEquals (v.Normal, firstNormal)))
				throw new ArgumentException ("All the normals need to point towards the same direction.", "frontFace");

			var edges = GetEdges (frontFace).ToArray ();
			var outerEdges = DetermineOuterEdges (edges).ToArray ();
			var geometries = new Geometry<V> [(outerEdges.Length * transforms.Count ())
			                 + BoolToInt (includeFrontFace) + BoolToInt (includeBackFace)];
			var backFace = frontFace;
            var i = 0;
			if (includeFrontFace)
            	geometries[i++] = frontFace;

			foreach (var transform in transforms)
            {
				backFace = frontFace.ManipulateVertices (v => v.With (transform.Transform (v.Position), -v.Normal));
				var backVertices = backFace.Vertices;
				i = ExtrudeOut (geometries, i, vertices, backVertices, outerEdges); 
                vertices = backVertices;
            }
            if (includeBackFace)
				geometries[i++] = backFace.ReverseWinding ();
            return Composite.Create (geometries);
		}

		public static Geometry<V> Extrude<V>(this Geometry<V> frontFace, float depth, 
			bool includeBackFace = true) where V : struct, IVertex
		{
			if (depth <= 0f)
				throw new ArgumentException (
					"Depth of the extrusion needs to be greater than zero.", "depth");
			var offs = -frontFace.Vertices[0].Normal * depth;
			return frontFace.Stretch (new Mat4[] { Mat.Translation<Mat4> (offs.X, offs.Y, offs.Z) },
				true, includeBackFace);
		}

		public static Geometry<V> Inset<V> (this Geometry<V> frontFace, float scaleX, float scaleY) 
			where V : struct, IVertex
		{
			var z = frontFace.Vertices.First ().Position.Z;
			if (!frontFace.Vertices.All (v => v.Position.Z == z))
				throw new ArgumentException (
					"All the vertices need to be on the XY-plane. I.e. they need to have the " +
					"same Z-coordinate.", "frontFace");
			return frontFace.Center ().Stretch (new Mat4[] { Mat.Scaling<Mat4> (scaleX, scaleY) },
				false, false).Simplify ();
		}

		public static Geometry<V> Extrude<V, P> (this IEnumerable<Path<P, Vec3>> paths,
			bool includeFrontFace, bool includeBackFace)
			where V : struct, IVertex
			where P : struct, IPositional<Vec3>
		{
			var frontFace = paths.First ();
			if (!paths.All (p => p.Nodes.Length >= 3))
				throw new ArgumentException ("All the paths must contain at least 3 vertices.", "paths");
			if (!paths.All (p => p.Nodes.Length == frontFace.Nodes.Length))
				throw new ArgumentException ("All the paths must contain the same number of vertices.", "paths");
			if (!paths.All (p => p.Nodes.AreCoplanar ()))
				throw new ArgumentException ("The nodes of the paths must be coplanar.", "paths");
			
			var vertices = GetVertices<V, P> (frontFace, false);
			var outerEdges = GetEdges (frontFace).ToArray ();
			var geometries = new Geometry<V> [(outerEdges.Length * (paths.Count () - 1)) + 
				BoolToInt (includeFrontFace) + BoolToInt (includeBackFace)];
			var i = 0;
			if (includeFrontFace)
				geometries[i++] = Polygon<V>.FromVertices (vertices);

			foreach (var backFace  in paths.Skip (1))
			{
				var backVertices = GetVertices<V, P> (backFace, true);
				i = ExtrudeOut (geometries, i, vertices, backVertices, outerEdges); 
				vertices = backVertices;
			}
			if (includeBackFace)
				geometries[i++] = Polygon<V>.FromVertices (vertices).ReverseWinding ();
			return Composite.Create (geometries);
		}

		public static Geometry<V> Extrude<V, P> (bool includeFrontFace, bool includeBackFace, params Path<P, Vec3>[] paths)
			where V : struct, IVertex
			where P : struct, IPositional<Vec3>
		{
			return paths.Extrude<V, P> (includeFrontFace, includeBackFace);
		}

		public static Geometry<V> Inset<P, V> (this Path<P, Vec3> path, float scaleX, float scaleY)
			where V : struct, IVertex
			where P : struct, IPositional<Vec3>
		{
			var z = path.Nodes.First ().Position.Z;
			if (!path.Nodes.All (p => p.Position.Z.ApproxEquals(z)))
				throw new ArgumentException (
					"All the nodes of the path need to be on the XY-plane. I.e. they need to have the " +
					"same Z-coordinate.", "path");

			return Extrude<V, P> (new Path<P, Vec3>[] { path, path.Scale (scaleX, scaleY) }, false, false)
				.ManipulateVertices (v => v.With (v.Position, new Vec3 (0f, 0f, 1f)))
				.Simplify ();
		}

		public static Geometry<V> BulgeOut<V> (this Geometry<V> plane, float depth, float flatness, float slope, 
			int numSteps, bool includeBackFace = true, Vec3 scaleAround = new Vec3 (), Axes scaleAxes = Axes.All) 
			where V : struct, IVertex
		{
			if (depth <= 0f)
				throw new ArgumentException (
					"Depth of bulge needs to be greater than zero.", "depth");
			if (flatness < 0 || flatness > 1)
				throw new ArgumentException (
					"Flatness parameter needs to be between 0 and 1.", "flatness");
			if (slope <= 0)
				throw new ArgumentException (
					"Slope parameter needs to be greater than zero.", "slope");

			var normal = plane.Vertices[0].Normal;
			var step = depth / numSteps;
			var stepVec = -plane.Vertices[0].Normal * step;
			var scaleRange = 1f - flatness;
			var transforms =
				from s in Ext.Range (0, depth, step)
				let factor = ((1f - (s / depth)) * scaleRange + flatness).Pow (slope)
				let factorx = scaleAxes.HasFlag (Axes.X) ? factor : 1f
				let factory = scaleAxes.HasFlag (Axes.Y) ? factor : 1f
				let offs = stepVec * s
				select Mat.Translation<Mat4> (offs.X, offs.Y, offs.Z) * 
					Mat.ScalingAlong (normal, new Vec2 (factorx, factory)).RelativeTo (scaleAround);
			return plane.Stretch (transforms, true, includeBackFace);
		}
		
		public static Geometry<V> Cube<V> (float width, float height, float depth) 
			where V : struct, IVertex
		{
			return Quadrilateral<V>.Rectangle (width, height).Extrude (depth);
		}
	}
}
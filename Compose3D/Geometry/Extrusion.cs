namespace Compose3D.Geometry
{
	using Maths;
    using System;
    using System.Collections.Generic;
    using System.Linq;
	using Extensions;
	using OpenTK.Graphics.OpenGL;

    public static class Extrusion
	{
		#region Private helper functions

		private static int BoolToInt (bool b)
		{
			return b ? 1 : 0;
		}

		private static V SideVertex<V> (V vertex, Vec3 normal)
			where V : struct, IVertex
		{
			return VertexHelpers.New<V> (vertex.position, normal);
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
			return vertices [index1].position.CalculateNormal (
				vertices [index2].position, backVertices [index1].position);
		}

		private static V[] GetVertices<V, P> (Path<P, Vec3> path, bool flipNormal)
			where V : struct, IVertex
			where P : struct, IPositional<Vec3>
		{
			var nodes = path.Nodes;
			var normal = nodes [1].position.CalculateNormal (nodes [0].position, nodes [2].position);
			if (flipNormal)
				normal = -normal;
			return nodes.Select (n => VertexHelpers.New<V> (n.position, normal)).ToArray ();
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
			var firstNormal = vertices[0].normal;
			if (!vertices.All (v => Vec.ApproxEquals (v.normal, firstNormal)))
				throw new ArgumentException ("All the normals need to point towards the same direction.", "frontFace");

			var edges = frontFace.GetEdges (BeginMode.Triangles).ToArray ();
			var outerEdges = DetermineOuterEdges (edges).ToArray ();
			var geometries = new Geometry<V> [(outerEdges.Length * transforms.Count ())
			                 + BoolToInt (includeFrontFace) + BoolToInt (includeBackFace)];
			var backFace = frontFace;
            var i = 0;
			if (includeFrontFace)
            	geometries[i++] = frontFace;

			foreach (var transform in transforms)
            {
				backFace = frontFace.ManipulateVertices (v => v.With (transform.Transform (v.position), -v.normal));
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
			var offs = -frontFace.Vertices[0].normal * depth;
			return frontFace.Stretch (new Mat4[] { Mat.Translation<Mat4> (offs.X, offs.Y, offs.Z) },
				true, includeBackFace);
		}

		public static Geometry<V> Inset<V> (this Geometry<V> frontFace, float scaleX, float scaleY) 
			where V : struct, IVertex
		{
			var z = frontFace.Vertices.First ().position.Z;
			if (!frontFace.Vertices.All (v => v.position.Z == z))
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
			var outerEdges = frontFace.GetEdges ().ToArray ();
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
			var z = path.Nodes.First ().position.Z;
			if (!path.Nodes.All (p => p.position.Z.ApproxEquals(z)))
				throw new ArgumentException (
					"All the nodes of the path need to be on the XY-plane. I.e. they need to have the " +
					"same Z-coordinate.", "path");

			return Extrude<V, P> (new Path<P, Vec3>[] { path, path.Scale (scaleX, scaleY) }, false, false)
				.ManipulateVertices (v => v.With (v.position, new Vec3 (0f, 0f, 1f)))
				.Simplify ();
		}

		public static Geometry<V> ExtrudeToScale<V> (this Geometry<V> plane, float depth, float targetScale, 
			float steepness, int numSteps, bool includeFrontFace = true, bool includeBackFace = true, 
			Vec3 scaleAround = new Vec3 ()) 
			where V : struct, IVertex
		{
			if (depth <= 0f)
				throw new ArgumentException (
					"Depth of bulge needs to be greater than zero.", "depth");
			if (steepness <= 0)
				throw new ArgumentException (
					"Slope parameter needs to be greater than zero.", "slope");

			var normal = plane.Vertices[0].normal;
			var step = depth / numSteps;
			var scaleRange = 1f - targetScale;
			var exponent = scaleRange < 0 ? 1f / steepness : steepness;
			var transforms =
				from s in EnumerableExt.Range (step, depth, step)
				let factor = (1f - (s / depth).Pow (exponent)) * scaleRange + targetScale
				let offs = -normal * s
				select Mat.Translation<Mat4> (offs.X, offs.Y, offs.Z) * 
					Mat.ScalingPerpendicularTo (normal, new Vec2 (factor)).RelativeTo (scaleAround);
			return plane.Stretch (transforms, includeFrontFace, includeBackFace);
		}
		
		public static Geometry<V> Cube<V> (float width, float height, float depth) 
			where V : struct, IVertex
		{
			return Quadrilateral<V>.Rectangle (width, height).Extrude (depth);
		}
	}
}
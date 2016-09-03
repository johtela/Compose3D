namespace Compose3D.Geometry
{
	using Compose3D.Maths;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;

	public class Polygon<V> : Primitive<V> where V : struct, IVertex
	{
		private int[] _indices;

		public Polygon (V[] vertices, int[] indices) : base (vertices)
		{
			_indices = indices;
		}

		public static Polygon<V> FromVertices (IEnumerable<V> vertices)
		{
			var path = vertices.Distinct ().ToArray ();
			if (path.Length < 3)
				throw new ArgumentException (
					"Polygon must contain at least 3 unique vertices. " +
					"Duplicate vertices are removed from the list automatically.", "vertices");
			return new Polygon<V> (path, Tesselator<V>.TesselatePolygon (path));
		}

		public static Polygon<V> FromVertices (params V[] vertices)
		{
			return Polygon<V>.FromVertices (vertices as IEnumerable<V>);
		}

		public static Polygon<V> FromVec2s (params Vec2[] vectors)
		{
			return FromVertices (vectors.Select (vec => VertexHelpers.New<V> (new Vec3 (vec, 0f), new Vec3 (0f, 0f, 1f))));
		}

		public static Polygon<V> FromPath<P> (Path<P, Vec3> Path)
			where P : struct, IPositional<Vec3>
		{
			return FromVertices (Path.Nodes.Select (n => VertexHelpers.New<V> (n.position, new Vec3 (0f, 0f, 1f))));
		}	

		protected override IEnumerable<int> GenerateIndices ()
		{
			return _indices;
		}
	}
}


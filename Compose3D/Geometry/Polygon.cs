namespace Compose3D.Geometry
{
	using Arithmetics;
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

		public static Polygon<V> FromVertices (params V[] vertices)
		{
			return new Polygon<V> (vertices, Tesselator<V>.TesselatePolygon (vertices));
		}

		public static Polygon<V> FromVec2s (params Vec2[] vectors)
		{
			return FromVertices (vectors.Map (vec => VertexHelpers.New<V> (new Vec3 (vec, 0f), new Vec3 (0f, 0f, 1f))));
		}

		protected override IEnumerable<int> GenerateIndices ()
		{
			return _indices;
		}
	}
}


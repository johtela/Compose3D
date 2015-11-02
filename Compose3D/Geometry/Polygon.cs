namespace Compose3D.Geometry
{
	using Arithmetics;
	using System.Collections.Generic;

	public class Polygon<V> : Primitive<V> where V : struct, IVertex
	{
		private int[] _indices;

		public Polygon (V[] vertices, int[] indices) : base (vertices)
		{
			_indices = indices;
		}

		public static Polygon<V> FromVertices (params V[] vertices)
		{
			var tess = new LibTessDotNet.Tess ();
			tess.AddContour (vertices.Map (v =>
			{
				var cv = new LibTessDotNet.ContourVertex ();
				cv.Position.X = v.Position.X;
				cv.Position.Y = v.Position.Y;
				cv.Position.Z = v.Position.Z;
				return cv;
			}), 
			LibTessDotNet.ContourOrientation.Clockwise);
			tess.Tessellate (LibTessDotNet.WindingRule.EvenOdd, LibTessDotNet.ElementType.Polygons, 3);
			return new Polygon<V> (vertices, tess.Elements);
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


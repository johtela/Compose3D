namespace Compose3D.Geometry
{
    using Arithmetics;
	using System;
    using System.Collections.Generic;

	internal class Quadrilateral<V> : Geometry<V> where V : struct, IVertex
	{
		private Func<Geometry<V>, V[]> _generateVertices;

		private Quadrilateral (Func<Geometry<V>, V[]> generateVertices)
		{
			_generateVertices = generateVertices;
		}

		public static Quadrilateral<V> FromVertices (params V[] vertices)
		{
			if (vertices.Length != 4)
				throw new GeometryError ("Quadrilaterals must have four vertices");
			if (!VertexHelpers.AreCoplanar (vertices))
				throw new GeometryError ("Vertices are not coplanar");
			return new Quadrilateral<V> (q => vertices);
		}

		public static Quadrilateral<V> Rectangle (float width, float height)
		{
			return Trapezoid (width, height, 0f, 0f);
		}

		public static Quadrilateral<V> Parallelogram (float width, float height, float topOffset)
		{
			return Trapezoid (width, height, topOffset, topOffset);
		}
			
		public static Quadrilateral<V> Trapezoid (float width, float height, 
			float topLeftOffset, float topRightOffset)
		{
			var right = width / 2f + topRightOffset;
			var top = height / 2f;
			var left = -right + topLeftOffset;
			var bottom = -top;
			var normal = new Vec3 (0f, 0f, 1f);
			return new Quadrilateral<V> (q =>
			{
				var colors = q.Material.Colors.GetEnumerator ();
				return new V[] 
				{
					Vertex (new Vec3 (right, top, 0f), colors.Next (), normal),
					Vertex (new Vec3 (right, bottom, 0f), colors.Next (), normal),
					Vertex (new Vec3 (left, bottom, 0f), colors.Next (), normal),
					Vertex (new Vec3 (left, top, 0f), colors.Next (), normal)
				};
			});
		}

		protected override IEnumerable<V> GenerateVertices ()
		{
			return _generateVertices (this);
		}

		protected override IEnumerable<int> GenerateIndices ()
		{
			return new int[] { 0, 1, 2, 2, 3, 0 };
		}
	}
}
namespace Compose3D.Geometry
{
	using System;
    using System.Collections.Generic;
	using Maths;

	public class Quadrilateral<V> : Primitive<V> where V : struct, IVertex3D
	{
		private Quadrilateral (V[] vertices) : base (vertices)
        { }

        public static Quadrilateral<V> FromVertices (params V[] vertices)
		{
			if (vertices.Length != 4)
				throw new ArgumentException ("Quadrilaterals must have four vertices", nameof (vertices));
			return new Quadrilateral<V> (vertices);
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
			var bottomRight = width / 2f;
			var topRight = bottomRight + topRightOffset;
			var top = height / 2f;
			var bottomLeft = -bottomRight;
			var topLeft = bottomLeft + topLeftOffset;
			var bottom = -top;
			var normal = new Vec3 (0f, 0f, 1f);
			return new Quadrilateral<V> (new V[] 
			{
				VertexHelpers.New<V> (new Vec3 (topRight, top, 0f), normal),
                VertexHelpers.New<V> (new Vec3 (bottomRight, bottom, 0f), normal),
                VertexHelpers.New<V> (new Vec3 (bottomLeft, bottom, 0f), normal),
                VertexHelpers.New<V> (new Vec3 (topLeft, top, 0f), normal)
			});
		}

		protected override IEnumerable<int> GenerateIndices ()
		{
			return new int[] { 0, 1, 2, 2, 3, 0 };
		}
	}
}
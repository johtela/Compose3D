namespace Compose3D.Geometry
{
    using Arithmetics;
	using System;
    using System.Collections.Generic;

	public class Quadrilateral<V> : Primitive<V> where V : struct, IVertex
	{
		private Quadrilateral (Func<Geometry<V>, V[]> generateVertices, IMaterial material)
            : base (generateVertices, material)
        { }

        public static Quadrilateral<V> FromVertices (IMaterial material, params V[] vertices)
		{
			if (vertices.Length != 4)
				throw new GeometryError ("Quadrilaterals must have four vertices");
			return new Quadrilateral<V> (q => vertices, material);
		}

		public static Quadrilateral<V> Rectangle (float width, float height, IMaterial material)
		{
			return Trapezoid (width, height, 0f, 0f, material);
		}

		public static Quadrilateral<V> Parallelogram (float width, float height, float topOffset, IMaterial material)
		{
			return Trapezoid (width, height, topOffset, topOffset, material);
		}
			
		public static Quadrilateral<V> Trapezoid (float width, float height, 
			float topLeftOffset, float topRightOffset, IMaterial material)
		{
			var bottomRight = width / 2f;
			var topRight = bottomRight + topRightOffset;
			var top = height / 2f;
			var bottomLeft = -bottomRight;
			var topLeft = bottomLeft + topLeftOffset;
			var bottom = -top;
			var normal = new Vec3 (0f, 0f, 1f);
			return new Quadrilateral<V> (q =>
			{
				var colors = q.Material.Colors.GetEnumerator ();
				return new V[] 
				{
					NewVertex (new Vec3 (topRight, top, 0f), colors.Next (), normal),
					NewVertex (new Vec3 (bottomRight, bottom, 0f), colors.Next (), normal),
					NewVertex (new Vec3 (bottomLeft, bottom, 0f), colors.Next (), normal),
					NewVertex (new Vec3 (topLeft, top, 0f), colors.Next (), normal)
				};
			}, material);
		}

		protected override IEnumerable<int> GenerateIndices ()
		{
			return new int[] { 0, 1, 2, 2, 3, 0 };
		}
	}
}
namespace Compose3D.Geometry
{
	using Arithmetics;
	using OpenTK;
	using System;
	using System.Collections.Generic;

	public class Triangle<V> : Primitive<V> where V : struct, IVertex
	{
		private Triangle (Func<Geometry<V>, V[]> generateVertices, IMaterial material)
            : base (generateVertices, material)
        { }

        public static Triangle<V> FromVertices (IMaterial material, params V[] vertices)
		{
			if (vertices.Length != 3)
				throw new GeometryError ("Triangles must have three vertices");
			if (!VertexHelpers.AreCoplanar (vertices))
				throw new GeometryError ("Vertices are not coplanar");
			return new Triangle<V> (t => vertices, material);
		}
			
		public static Triangle<V> Equilateral (float width, IMaterial material)
		{
			return Isosceles (width, width * (float)Math.Sin (MathHelper.PiOver6), material);
		}

		public static Triangle<V> Isosceles (float width, float height, IMaterial material)
		{
			var offs = width / 2f;
			return Scalene (-offs, offs, height, material);
		}

		public static Triangle<V> Scalene (float leftOffset, float rightOffset, float height, IMaterial material)
		{
			var normal = new Vec3 (0f, 0f, -1f);
			return new Triangle<V> (q =>
			{
				var colors = q.Material.Colors.GetEnumerator ();
				return new V[] 
				{
					NewVertex (new Vec3 (0f, height, 0f), colors.Next (), normal),
					NewVertex (new Vec3 (rightOffset, 0f, 0f), colors.Next (), normal),
					NewVertex (new Vec3 (leftOffset, 0f, 0f), colors.Next (), normal)
				};
			}, material);
		}

		protected override IEnumerable<int> GenerateIndices ()
		{
			return new int[] { 0, 1, 2 };
		}
	}
}


namespace Compose3D.Geometry
{
	using Compose3D.Maths;
    using Textures;
	using OpenTK;
	using System;
	using System.Collections.Generic;

	public class Triangle<V> : Primitive<V> where V : struct, IVertex
	{
		private Triangle (V[] vertices) : base (vertices)
        { }

        public static Triangle<V> FromVertices (params V[] vertices)
		{
			if (vertices.Length != 3)
				throw new GeometryError ("Triangles must have three vertices");
			return new Triangle<V> (vertices);
		}
			
		public static Triangle<V> Equilateral (float width)
		{
			return Isosceles (width, width * (float)Math.Sin (MathHelper.PiOver6));
		}

		public static Triangle<V> Isosceles (float width, float height)
		{
			var offs = width / 2f;
			return Scalene (-offs, offs, height);
		}

		public static Triangle<V> Scalene (float leftOffset, float rightOffset, float height)
		{
			var normal = new Vec3 (0f, 0f, 1f);
 			return new Triangle<V> (new V[] 
			{
                VertexHelpers.New<V> (new Vec3 (0f, height, 0f), normal),
                VertexHelpers.New<V> (new Vec3 (rightOffset, 0f, 0f), normal),
                VertexHelpers.New<V> (new Vec3 (leftOffset, 0f, 0f), normal)
			});
		}

		protected override IEnumerable<int> GenerateIndices ()
		{
			return new int[] { 0, 1, 2 };
		}
	}
}


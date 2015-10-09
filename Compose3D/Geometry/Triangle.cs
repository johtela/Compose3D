namespace Compose3D.Geometry
{
	using Arithmetics;
    using Textures;
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
			var normal = new Vec3 (0f, 0f, 1f);
			var width = rightOffset - leftOffset;
 			return new Triangle<V> (q =>
			{
				var vertexMaterials = q.Material.VertexMaterials.GetEnumerator ();
				return new V[] 
				{
					NewVertex (new Vec3 (0f, height, 0f), normal, 
						new Vec2 ((-leftOffset / width).Clamp (0f, 1f), 1f), vertexMaterials.Next ()),
					NewVertex (new Vec3 (rightOffset, 0f, 0f), normal, TexturePos.BottomRight, vertexMaterials.Next ()),
					NewVertex (new Vec3 (leftOffset, 0f, 0f), normal, TexturePos.BottomLeft, vertexMaterials.Next ())
				};
			}, material);
		}

		protected override IEnumerable<int> GenerateIndices ()
		{
			return new int[] { 0, 1, 2 };
		}
	}
}


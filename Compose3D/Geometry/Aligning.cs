namespace Compose3D.Geometry
{
	using Arithmetics;
	using System.Collections.Generic;
	using System.Linq;

	public static class Aligning
	{
		private static Mat4 GetAlignmentMatrix (Alignment xalign, Alignment yalign, Alignment zalign, 
			BBox previous, BBox current)
		{
			return Mat.Translation<Mat4> (
				previous.GetXOffset (current, xalign),
				previous.GetYOffset (current, yalign),
				previous.GetZOffset (current, zalign));
		}

		public static IEnumerable<Geometry<V>> Align<V> (this IEnumerable<Geometry<V>> geometries, 
			Alignment xalign, Alignment yalign, Alignment zalign) where V : struct, IVertex
		{
			var previous = geometries.First ().BoundingBox;
			var alignedGeometries = geometries.Skip (1).Select (geom => 
			{
				var current = geom.BoundingBox;
				var matrix = GetAlignmentMatrix (xalign, yalign, zalign, previous, current);
				previous = new BBox (
					new Vec3 (matrix * new Vec4 (current.Min, 1f)), 
					new Vec3 (matrix * new Vec4 (current.Max, 1f)));
				return Geometry.Transform (geom, matrix);
			});
			return geometries.Take (1).Concat (alignedGeometries);
		}

		public static IEnumerable<Geometry<V>> Align<V> (Alignment xalign, Alignment yalign, Alignment zalign,
			params Geometry<V>[] geometries) where V : struct, IVertex
		{
			return geometries.Align (xalign, yalign, zalign);
		}
	}
}


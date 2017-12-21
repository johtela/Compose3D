namespace Compose3D.Geometry
{
	using Compose3D.Maths;
	using System.Collections.Generic;
	using System.Linq;
	using DataStructures;

	public static class Aligning
	{
		private static Mat4 GetAlignmentMatrix (Alignment xalign, Alignment yalign, Alignment zalign,
			Aabb<Vec3> alignWith, Aabb<Vec3> bbox)
		{
			return Mat.Translation<Mat4> (
				alignWith.GetAlignmentOffset (bbox, 0, xalign),
				alignWith.GetAlignmentOffset (bbox, 1, yalign),
				alignWith.GetAlignmentOffset (bbox, 2, zalign));
		}

		public static IEnumerable<Geometry<V>> Align<V> (this IEnumerable<Geometry<V>> geometries, 
			Alignment xalign, Alignment yalign, Alignment zalign) where V : struct, IVertex3D
		{
			var alignWith = geometries.First ().BoundingBox;
			var alignedGeometries = geometries.Skip (1).Select (geom => 
				geom.Transform (GetAlignmentMatrix (xalign, yalign, zalign, alignWith, geom.BoundingBox)));
			return geometries.Take (1).Concat (alignedGeometries);
		}

		public static IEnumerable<Geometry<V>> Align<V> (Alignment xalign, Alignment yalign, Alignment zalign,
			params Geometry<V>[] geometries) where V : struct, IVertex3D
		{
			return geometries.Align (xalign, yalign, zalign);
		}

		public static IEnumerable<Geometry<V>> AlignX<V> (this IEnumerable<Geometry<V>> geometries, Alignment xalign) 
			where V : struct, IVertex3D
		{
			return geometries.Align (xalign, Alignment.None, Alignment.None);
		}

		public static IEnumerable<Geometry<V>> AlignX<V> (Alignment xalign, params Geometry<V>[] geometries) 
			where V : struct, IVertex3D
		{
			return geometries.AlignX (xalign);
		}

		public static IEnumerable<Geometry<V>> AlignY<V> (this IEnumerable<Geometry<V>> geometries, Alignment yalign) 
			where V : struct, IVertex3D
		{
			return geometries.Align (Alignment.None, yalign, Alignment.None);
		}

		public static IEnumerable<Geometry<V>> AlignY<V> (Alignment yalign, params Geometry<V>[] geometries) 
			where V : struct, IVertex3D
		{
			return geometries.AlignY (yalign);
		}

		public static IEnumerable<Geometry<V>> AlignZ<V> (this IEnumerable<Geometry<V>> geometries, Alignment zalign) 
			where V : struct, IVertex3D
		{
			return geometries.Align (Alignment.None, Alignment.None, zalign);
		}

		public static IEnumerable<Geometry<V>> AlignZ<V> (Alignment zalign, params Geometry<V>[] geometries) 
			where V : struct, IVertex3D
		{
			return geometries.AlignZ (zalign);
		}
	}
}


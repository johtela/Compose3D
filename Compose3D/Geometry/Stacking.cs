namespace Compose3D.Geometry
{
	using Compose3D.Maths;
	using System.Collections.Generic;
	using System.Linq;
	using DataStructures;

	public static class Stacking
	{
		private static Mat4 GetStackingMatrix (Axis axis, AxisDirection direction, Aabb<Vec3> previous, Aabb<Vec3> current)
		{
			switch (axis)
			{
				case Axis.X: 
					return Mat.Translation<Mat4> (direction == AxisDirection.Positive ? 
						previous.Right - current.Left : previous.Left - current.Right, 0f, 0f);
				case Axis.Y: 
					return Mat.Translation<Mat4> (0f, direction == AxisDirection.Positive ? 
						previous.Top - current.Bottom : previous.Bottom - current.Top, 0f);
				default: 
					return Mat.Translation<Mat4> (0f, 0f, direction == AxisDirection.Positive ? 
						previous.Front - current.Back : previous.Back - current.Front);
			}
		}

		public static IEnumerable<Geometry<V>> Stack<V> (this IEnumerable<Geometry<V>> geometries, 
			Axis axis, AxisDirection direction) where V : struct, IVertex
		{
			var previous = geometries.First ().BoundingBox;
			var stackedGeometries = geometries.Skip (1).Select (geom => 
			{
				var current = geom.BoundingBox;
				var matrix = GetStackingMatrix (axis, direction, previous, current);
				previous = new Aabb<Vec3> (
					new Vec3 (matrix * new Vec4 (current.Min, 1f)), 
					new Vec3 (matrix * new Vec4 (current.Max, 1f)));
				return geom.Transform (matrix);
			});
			return geometries.Take (1).Concat (stackedGeometries);
		}

		public static IEnumerable<Geometry<V>> Stack<V> (Axis axis, AxisDirection direction, 
			params Geometry<V>[] geometries) where V : struct, IVertex
		{
			return geometries.Stack (axis, direction);
		}

		public static IEnumerable<Geometry<V>> StackLeft<V> (this IEnumerable<Geometry<V>> geometries) 
			where V : struct, IVertex
		{
			return geometries.Stack (Axis.X, AxisDirection.Negative);
		}

		public static IEnumerable<Geometry<V>> StackLeft<V> (params Geometry<V>[] geometries)
			where V : struct, IVertex
		{
			return geometries.StackLeft ();
		}

		public static IEnumerable<Geometry<V>> StackRight<V> (this IEnumerable<Geometry<V>> geometries)
			where V : struct, IVertex
		{
			return geometries.Stack (Axis.X, AxisDirection.Positive);
		}

		public static IEnumerable<Geometry<V>> StackRight<V> (params Geometry<V>[] geometries)
			where V : struct, IVertex
		{
			return geometries.StackRight ();
		}

		public static IEnumerable<Geometry<V>> StackDown<V> (this IEnumerable<Geometry<V>> geometries)
			where V : struct, IVertex
		{
			return geometries.Stack (Axis.Y, AxisDirection.Negative);
		}

		public static IEnumerable<Geometry<V>> StackDown<V> (params Geometry<V>[] geometries)
			where V : struct, IVertex
		{
			return geometries.StackDown ();
		}

		public static IEnumerable<Geometry<V>> StackUp<V> (this IEnumerable<Geometry<V>> geometries)
			where V : struct, IVertex
		{
			return geometries.Stack (Axis.Y, AxisDirection.Positive);
		}

		public static IEnumerable<Geometry<V>> StackUp<V> (params Geometry<V>[] geometries)
			where V : struct, IVertex
		{
			return geometries.StackUp ();
		}

		public static IEnumerable<Geometry<V>> StackBackward<V> (this IEnumerable<Geometry<V>> geometries)
			where V : struct, IVertex
		{
			return geometries.Stack (Axis.Z, AxisDirection.Negative);
		}

		public static IEnumerable<Geometry<V>> StackBackward<V> (params Geometry<V>[] geometries)
			where V : struct, IVertex
		{
			return geometries.StackBackward ();
		}

		public static IEnumerable<Geometry<V>> StackForward<V> (this IEnumerable<Geometry<V>> geometries)
			where V : struct, IVertex
		{
			return geometries.Stack (Axis.Z, AxisDirection.Positive);
		}

		public static IEnumerable<Geometry<V>> StackForward<V> (params Geometry<V>[] geometries)
			where V : struct, IVertex
		{
			return geometries.StackForward ();
		}
	}
}


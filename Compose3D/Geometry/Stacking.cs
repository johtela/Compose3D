namespace Compose3D.Geometry
{
	using Arithmetics;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Axis along which the geometries are stacked.
	/// </summary>
	public enum StackAxis { X, Y, Z }

	/// <summary>
	/// Direction of the stack; towards negative or positive values.
	/// </summary>
	public enum StackDirection : int
	{ 
		Negative = -1, 
		Positive = 1
	}

	public static class Stacking
	{
		private static Mat4 GetStackingMatrix (StackAxis axis, StackDirection direction, BBox previous, BBox current)
		{
			switch (axis)
			{
			case StackAxis.X: 
				return Mat.Translation<Mat4> ((previous.Right - current.Left) * (float)direction, 0f, 0f);
			case StackAxis.Y: 
				return Mat.Translation<Mat4> (0f, (previous.Top - current.Bottom) * (float)direction, 0f);
			default: 
				return Mat.Translation<Mat4> (0f, 0f, (previous.Front - current.Back) * (float)direction);
			}
		}

		public static IEnumerable<Geometry<V>> Stack<V> (this IEnumerable<Geometry<V>> geometries, 
			StackAxis axis, StackDirection direction) where V : struct, IVertex
		{
			var previous = geometries.First ().BoundingBox;
			var stackedGeometries = geometries.Skip (1).Select (geom => 
			{
				var current = geom.BoundingBox;
				var matrix = GetStackingMatrix (axis, direction, previous, current);
				previous = new BBox (
					new Vec3 (matrix * new Vec4 (current.Min, 1f)), 
					new Vec3 (matrix * new Vec4 (current.Max, 1f)));
				return Geometry.Transform (geom, matrix);
			});
			return geometries.Take (1).Concat (stackedGeometries);
		}

		public static IEnumerable<Geometry<V>> Stack<V> (StackAxis axis, StackDirection direction, 
			params Geometry<V>[] geometries) where V : struct, IVertex
		{
			return geometries.Stack (axis, direction);
		}

		public static IEnumerable<Geometry<V>> StackLeft<V> (this IEnumerable<Geometry<V>> geometries) 
			where V : struct, IVertex
		{
			return geometries.Stack (StackAxis.X, StackDirection.Negative);
		}

		public static IEnumerable<Geometry<V>> StackLeft<V> (params Geometry<V>[] geometries)
			where V : struct, IVertex
		{
			return geometries.StackLeft ();
		}

		public static IEnumerable<Geometry<V>> StackRight<V> (this IEnumerable<Geometry<V>> geometries)
			where V : struct, IVertex
		{
			return geometries.Stack (StackAxis.X, StackDirection.Positive);
		}

		public static IEnumerable<Geometry<V>> StackRight<V> (params Geometry<V>[] geometries)
			where V : struct, IVertex
		{
			return geometries.StackRight ();
		}

		public static IEnumerable<Geometry<V>> StackDown<V> (this IEnumerable<Geometry<V>> geometries)
			where V : struct, IVertex
		{
			return geometries.Stack (StackAxis.Y, StackDirection.Negative);
		}

		public static IEnumerable<Geometry<V>> StackDown<V> (params Geometry<V>[] geometries)
			where V : struct, IVertex
		{
			return geometries.StackDown ();
		}

		public static IEnumerable<Geometry<V>> StackUp<V> (this IEnumerable<Geometry<V>> geometries)
			where V : struct, IVertex
		{
			return geometries.Stack (StackAxis.Y, StackDirection.Positive);
		}

		public static IEnumerable<Geometry<V>> StackUp<V> (params Geometry<V>[] geometries)
			where V : struct, IVertex
		{
			return geometries.StackUp ();
		}

		public static IEnumerable<Geometry<V>> StackBackward<V> (this IEnumerable<Geometry<V>> geometries)
			where V : struct, IVertex
		{
			return geometries.Stack (StackAxis.Z, StackDirection.Negative);
		}

		public static IEnumerable<Geometry<V>> StackBackward<V> (params Geometry<V>[] geometries)
			where V : struct, IVertex
		{
			return geometries.StackBackward ();
		}

		public static IEnumerable<Geometry<V>> StackForward<V> (this IEnumerable<Geometry<V>> geometries)
			where V : struct, IVertex
		{
			return geometries.Stack (StackAxis.Z, StackDirection.Positive);
		}

		public static IEnumerable<Geometry<V>> StackForward<V> (Alignment xalign, Alignment yalign, params Geometry<V>[] geometries)
			where V : struct, IVertex
		{
			return geometries.StackForward ();
		}
	}
}


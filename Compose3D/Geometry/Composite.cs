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

	/// <summary>
	/// Basic building block for composite geometries.
	/// </summary>
	internal class Composite<V> : Geometry<V> where V : struct, IVertex
	{
		private Geometry<V>[] _geometries;
		private int? _vertexCount;

		public Composite (params Geometry<V>[] geometries)
		{
			_geometries = geometries;
		}

		public Composite (IEnumerable<Geometry<V>> geometries)
		{
			_geometries = geometries.ToArray ();
		}

		public override int VertexCount
		{
			get
			{
				if (!_vertexCount.HasValue)
					_vertexCount = _geometries.Sum (g => g.VertexCount);
				return _vertexCount.Value;
			}
		}

		public override IEnumerable<V> Vertices
		{
			get { return _geometries.SelectMany (g => g.Vertices); }
		}

		public override IEnumerable<int> Indices
		{
			get
			{
				for (int g = 0, c = 0; g < _geometries.Length; g++)
				{
					foreach (var i in _geometries[g].Indices)
						yield return c + i;
					c += _geometries[g].VertexCount;
				}
			}
		}

		public override IMaterial Material
		{
			get { return base.Material; }
			set
			{
				foreach (var geometry in _geometries)
					if (!geometry.HasMaterial)
						geometry.Material = value;
				base.Material = value;
			}
		}
	}

	/// <summary>
	/// Helper methods to create various composite geometries.
	/// </summary>
	public static class Composite
	{
		public static Geometry<V> Create<V> (params Geometry<V>[] geometries) where V : struct, IVertex
		{
			return new Composite<V> (geometries);
		}

		public static Geometry<V> Create<V> (IEnumerable<Geometry<V>> geometries) where V : struct, IVertex
		{
			return new Composite<V> (geometries);
		}

		private static Mat4 GetOffsetMatrix (StackAxis axis, StackDirection direction, BBox previous, BBox current)
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

		private static Mat4 GetStackingMatrix (StackAxis axis, StackDirection direction, 
      		Align xalign, Align yalign, Align zalign, BBox previous, BBox current)
		{
			var alignMatrix = Mat.Translation<Mat4> (previous.GetXOffset (current, xalign),
			                                         previous.GetYOffset (current, yalign),
			                                         previous.GetZOffset (current, zalign));
			return alignMatrix * GetOffsetMatrix (axis, direction, previous, current);
		}

		public static Geometry<V> Stack<V> (StackAxis axis, StackDirection direction,
            Align xalign, Align yalign, Align zalign, IEnumerable<Geometry<V>> geometries) where V : struct, IVertex
		{
			var previous = geometries.First ().BoundingBox;
			var stackedGeometries = geometries.Skip (1).Select (geom => 
			{
				var current = geom.BoundingBox;
				var matrix = GetStackingMatrix (axis, direction, xalign, yalign, zalign, previous, current);
				previous = new BBox (new Vec3 (matrix * new Vec4 (current.Position, 1f)), current.Size);
				return Geometry.Transform (geom, matrix);
			});
			return Create (geometries.Take (1).Concat (stackedGeometries));
		}

		public static Geometry<V> Stack<V> (StackAxis axis, StackDirection direction,
            Align xalign, Align yalign, Align zalign, params Geometry<V>[] geometries) where V : struct, IVertex
		{
			return Stack (axis, direction, xalign, yalign, zalign, geometries as IEnumerable<Geometry<V>>);
		}

		public static Geometry<V> StackLeft<V> (Align yalign, Align zalign, IEnumerable<Geometry<V>> geometries)
			where V : struct, IVertex
		{
			return Stack (StackAxis.X, StackDirection.Negative, Align.None, yalign, zalign, geometries);
		}

		public static Geometry<V> StackLeft<V> (Align yalign, Align zalign, params Geometry<V>[] geometries)
			where V : struct, IVertex
		{
			return Stack (StackAxis.X, StackDirection.Negative, Align.None, yalign, zalign, 
			              geometries as IEnumerable<Geometry<V>>);
		}

		public static Geometry<V> StackRight<V> (Align yalign, Align zalign, IEnumerable<Geometry<V>> geometries)
			where V : struct, IVertex
		{
			return Stack (StackAxis.X, StackDirection.Positive, Align.None, yalign, zalign, geometries);
		}

		public static Geometry<V> StackRight<V> (Align yalign, Align zalign, params Geometry<V>[] geometries)
			where V : struct, IVertex
		{
			return Stack (StackAxis.X, StackDirection.Positive, Align.None, yalign, zalign, 
			              geometries as IEnumerable<Geometry<V>>);
		}

		public static Geometry<V> StackDown<V> (Align xalign, Align zalign, IEnumerable<Geometry<V>> geometries)
			where V : struct, IVertex
		{
			return Stack (StackAxis.Y, StackDirection.Negative, xalign, Align.None, zalign, geometries);
		}

		public static Geometry<V> StackDown<V> (Align xalign, Align zalign, params Geometry<V>[] geometries)
			where V : struct, IVertex
		{
			return Stack (StackAxis.Y, StackDirection.Negative, xalign, Align.None, zalign, 
			              geometries as IEnumerable<Geometry<V>>);
		}

		public static Geometry<V> StackUp<V> (Align xalign, Align zalign, IEnumerable<Geometry<V>> geometries)
			where V : struct, IVertex
		{
			return Stack (StackAxis.Y, StackDirection.Positive, xalign, Align.None, zalign, geometries);
		}

		public static Geometry<V> StackUp<V> (Align xalign, Align zalign, params Geometry<V>[] geometries)
			where V : struct, IVertex
		{
			return Stack (StackAxis.Y, StackDirection.Positive, xalign, Align.None, zalign, 
			              geometries as IEnumerable<Geometry<V>>);
		}

		public static Geometry<V> StackBackward<V> (Align xalign, Align yalign, IEnumerable<Geometry<V>> geometries)
			where V : struct, IVertex
		{
			return Stack (StackAxis.Z, StackDirection.Negative, xalign, yalign, Align.None, geometries);
		}

		public static Geometry<V> StackBackward<V> (Align xalign, Align yalign, params Geometry<V>[] geometries)
			where V : struct, IVertex
		{
			return Stack (StackAxis.Z, StackDirection.Negative, xalign, yalign, Align.None, 
			              geometries as IEnumerable<Geometry<V>>);
		}

		public static Geometry<V> StackForward<V> (Align xalign, Align yalign, IEnumerable<Geometry<V>> geometries)
			where V : struct, IVertex
		{
			return Stack (StackAxis.Z, StackDirection.Positive, xalign, yalign, Align.None, geometries);
		}

		public static Geometry<V> StackForward<V> (Align xalign, Align yalign, params Geometry<V>[] geometries)
			where V : struct, IVertex
		{
			return Stack (StackAxis.Z, StackDirection.Positive, xalign, yalign, Align.None, 
			              geometries as IEnumerable<Geometry<V>>);
		}
	}
}

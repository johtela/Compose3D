namespace Visual3D.Geometry
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;

	/// <summary>
	/// Axis along which the geometries are stacked.
	/// </summary>
	public enum StackAxis { X, Y, Z }

	/// <summary>
	/// Direction of the stack; towards negative or positive values.
	/// </summary>
	public enum StackDirection { Negative, Positive }

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

		private static  Matrix4 GetOffsetMatrix (StackAxis axis, StackDirection direction, BBox previous, BBox current)
		{
			switch (axis)
			{
				case StackAxis.X: 
					return direction == StackDirection.Negative ? 
						Matrix4.CreateTranslation (previous.Left - current.Right, 0.0f, 0.0f) :
						Matrix4.CreateTranslation (previous.Right - current.Left, 0.0f, 0.0f);
				case StackAxis.Y: 
					return direction == StackDirection.Negative ? 
						Matrix4.CreateTranslation (0.0f, previous.Bottom - current.Top, 0.0f) :
						Matrix4.CreateTranslation (0.0f, previous.Top - current.Bottom, 0.0f);
				default: 
					return direction == StackDirection.Negative ? 
						Matrix4.CreateTranslation (0.0f, 0.0f, previous.Back - current.Front) :
						Matrix4.CreateTranslation (0.0f, 0.0f, previous.Front - current.Back);
			}
		}

		private static Matrix4 GetStackingMatrix (StackAxis axis, StackDirection direction, 
		                                          Align xalign, Align yalign, Align zalign,
		                                          BBox previous, BBox current)
		{
			var alignMatrix = Matrix4.CreateTranslation (previous.GetXOffset (current, xalign),
			                                             previous.GetYOffset (current, yalign),
			                                             previous.GetZOffset (current, zalign));
			return alignMatrix * GetOffsetMatrix (axis, direction, previous, current);
		}

		public static Geometry<V> Stack<V> (StackAxis axis, StackDirection direction,
		                                    Align xalign, Align yalign, Align zalign,
		                                    IEnumerable<Geometry<V>> geometries) where V : struct, IVertex
		{
			var previous = geometries.First ().BoundingBox;
			var stackedGeometries = geometries.Skip (1).Select (geom => 
			{
				var current = geom.BoundingBox;
				var matrix = GetStackingMatrix (axis, direction, xalign, yalign, zalign, previous, current);
				previous = new BBox (Vector3.Transform (current.Position, matrix), current.Size);
				return Geometry.Transform (geom, matrix);
			});
			return Create (stackedGeometries);
		}

		public static Geometry<V> Stack<V> (StackAxis axis, StackDirection direction,
		                                    Align xalign, Align yalign, Align zalign,
		                                    params Geometry<V>[] geometries) where V : struct, IVertex
		{
			return Stack (axis, direction, xalign, yalign, zalign, geometries as IEnumerable<Geometry<V>>);
		}
	}
}

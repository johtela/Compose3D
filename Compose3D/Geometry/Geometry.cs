namespace Compose3D.Geometry
{
    using Arithmetics;
    using System;
    using System.Collections.Generic;
	using System.Linq;

	public class GeometryError : Exception
	{
		public GeometryError (string msg) : base (msg) { }
	}

	/// <summary>
	/// Abstraction for geometrical shapes that can be rendered with OpenGL.
	/// </summary>
	/// <description>
	/// Complex geometries are created by composing simple primitives together.
	/// </description>
	public abstract class Geometry<V> where V : struct, IVertex
	{
		private BBox _boundingBox;
		private V[] _vertices;
		private int[] _indices;

		private V[] CachedVertices ()
		{
			if (_vertices == null)
				_vertices = GenerateVertices ().ToArray ();
			return _vertices;
		}

		private int[] CachedIndices ()
		{
			if (_indices == null)
				_indices = GenerateIndices ().ToArray ();
			return _indices;
		}

		protected abstract IEnumerable<V> GenerateVertices ();
		protected abstract IEnumerable<int> GenerateIndices ();
		public abstract IMaterial Material { get; }

		/// <summary>
		/// Enumerates the vertices of the geometry.
		/// </summary>
		public V[] Vertices 
		{ 
			get { return CachedVertices (); }
		}

		/// <summary>
		/// Enumerates the indices used to draw triangles of the geometry.
		/// </summary>
		public int[] Indices 
		{ 
			get { return CachedIndices (); }
		}

		/// <summary>
		/// Return the bounding box of this geometry.
		/// </summary>
		public  virtual BBox BoundingBox 
		{
			get
			{
				if (_boundingBox == null)
				{
					if (Vertices.Length < 1)
						throw new GeometryError ("Geometry must contain at least one vertex");
					_boundingBox = new BBox (Vertices[0].Position);
					for (int i = 0; i < Vertices.Length; i++)
						_boundingBox += Vertices[i].Position;
				}
				return _boundingBox;
			}
		}

		/// <summary>
		/// Helper function that creates a vertex and sets its position and color.
		/// </summary>
		public static V Vertex (Vec3 position, Vec4 color, Vec3 normal)
		{
			var vertex = new V ();
			vertex.Position = position;
			vertex.Color = color;
            vertex.Normal = normal;
			return vertex;
		}
	}

	/// <summary>
	/// Contains static and extension methods that can be used compose geometries.
	/// </summary>
	public static class Geometry
	{
		public static Geometry<V> ReverseIndices<V> (this Geometry<V> geometry) 
			where V : struct, IVertex
		{
			return new ReverseIndices<V> (geometry);
		}

		public static Geometry<V> Transform<V> (this Geometry<V> geometry, Mat4 matrix) 
			where V : struct, IVertex
		{
			return new Transform<V> (geometry, matrix);
		}

		public static Geometry<V> Translate<V> (this Geometry<V> geometry, float offsetX, float offsetY, float offsetZ)
			where V : struct, IVertex
		{
			var matrix = Mat.Translation<Mat4> (offsetX, offsetY, offsetZ);
			return Transform (geometry, matrix);
		}

		public static Geometry<V> Scale<V> (this Geometry<V> geometry, float factorX, float factorY, float factorZ)
			where V : struct, IVertex
		{
			var matrix = Mat.Scaling<Mat4> (factorX, factorY, factorZ);
			return Transform (geometry, matrix);
		}

		public static Geometry<V> Rotate<V> (this Geometry<V> geometry, float angleX, float angleY, float angleZ)
			where V : struct, IVertex
		{
			var matrix = Mat.RotationZ<Mat4> (angleZ);
			if (angleX != 0.0f) matrix *= Mat.RotationX<Mat4> (angleX);
			if (angleY != 0.0f) matrix *= Mat.RotationY<Mat4> (angleY);
			return Transform (geometry, matrix);
		}

		public static Geometry<V> ReflectX<V> (this Geometry<V> geometry)
			where V : struct, IVertex
		{
			return geometry.Scale (-1f, 0f, 0f).ReverseIndices ();
		}

		public static Geometry<V> ReflectY<V> (this Geometry<V> geometry)
			where V : struct, IVertex
		{
			return geometry.Scale (0f, -1f, 0f).ReverseIndices ();
		}

		public static Geometry<V> ReflectZ<V> (this Geometry<V> geometry)
			where V : struct, IVertex
		{
			return geometry.Scale (0f, 0f, -1f).ReverseIndices ();
		}

		public static Geometry<V> Center<V> (this Geometry<V> geometry) where V : struct, IVertex
		{
			var center = geometry.BoundingBox.Center;
			return Translate (geometry, -center.X, -center.Y, -center.Z);
		}
	}
}

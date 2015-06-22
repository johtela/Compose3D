namespace Compose3D.Geometry
{
    using Arithmetics;
    using System;
    using System.Collections.Generic;

	public class GeometryError : Exception
	{
		public GeometryError (string msg) : base (msg) { }
	}

	/// <summary>
	/// Interface that is used to access mandatory vertex attributes.
	/// </summary>
	/// <description>
	/// All Vertex structures need to implement this interface. Through it
	/// geometry generators can set vertex attributes.
	/// </description>
	public interface IVertex
	{
		/// <summary>
		/// Position of the vertex.
		/// </summary>
		Vec3 Position { get; set; }

		/// <summary>
		/// Color of the vertex.
		/// </summary>
		Vec4 Color { get; set; }

        /// <summary>
        /// The normal of the vertex.
        /// </summary>
        Vec3 Normal { get; set; }
	}

	/// <summary>
	/// Abstraction for geometrical shapes that can be rendered with OpenGL.
	/// </summary>
	/// <description>
	/// Complex geometries are created by composing simple primitives together.
	/// </description>
	public abstract class Geometry<V> where V : struct, IVertex
	{
		private IMaterial _material;
		private BBox _boundingBox;

		/// <summary>
		/// Gets the number of vertices in this geometry.
		/// </summary>
		public abstract int VertexCount { get; }

		/// <summary>
		/// Enumerates the vertices of the geometry.
		/// </summary>
		public abstract IEnumerable<V> Vertices { get; }

		/// <summary>
		/// Enumerates the indices used to draw triangles of the geometry.
		/// </summary>
		public abstract IEnumerable<int> Indices { get; }

		/// <summary>
		/// Return the bounding box of this geometry.
		/// </summary>
		public  virtual BBox BoundingBox 
		{
			get
			{
				if (_boundingBox == null)
				{
					var e = Vertices.GetEnumerator ();
					if (!e.MoveNext ())
						throw new GeometryError ("Geometry must contain at least one vertex");
					_boundingBox = new BBox (e.Current.Position);
					while (e.MoveNext ())
						_boundingBox += e.Current.Position;
				}
				return _boundingBox;
			}
		}

		/// <summary>
		/// Gets the material used for the geometry.
		/// </summary>
		public virtual IMaterial Material 
		{ 
			get 
			{ 
				if (_material == null) throw new GeometryError("Material not set for this geometry");
				return _material;
			}
			set { _material = value; }
		}

		/// <summary>
		/// Gets a value indicating whether this geometry has material set.
		/// </summary>
		public bool HasMaterial
		{
			get { return _material != null; }
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
		public static Geometry<V> Rectangle<V> (float width, float height) where V : struct, IVertex
		{
			return Quadrilateral<V>.Rectangle (width, height);
		}

		public static Geometry<V> Transform<V> (this Geometry<V> geometry, Mat4 matrix) where V : struct, IVertex
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
			return geometry.Scale (-1f, 0f, 0f);
		}

		public static Geometry<V> ReflectY<V> (this Geometry<V> geometry)
			where V : struct, IVertex
		{
			return geometry.Scale (0f, -1f, 0f);
		}

		public static Geometry<V> ReflectZ<V> (this Geometry<V> geometry)
			where V : struct, IVertex
		{
			return geometry.Scale (0f, 0f, -1f);
		}

		public static Geometry<V> Center<V> (this Geometry<V> geometry) where V : struct, IVertex
		{
			var center = geometry.BoundingBox.Center;
			return Translate (geometry, -center.X, -center.Y, -center.Z);
		}

		public static Geometry<V> Material<V> (this Geometry<V> geometry, IMaterial material) where V : struct, IVertex
		{
			geometry.Material = material;
			return geometry;
		}
	}
}

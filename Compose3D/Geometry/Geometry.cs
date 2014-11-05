namespace Compose3D.Geometry
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;

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
		Vector4 Position { get; set; }

		/// <summary>
		/// Color of the vertex.
		/// </summary>
		Vector4 Color { get; set; }
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
				var e = Vertices.GetEnumerator ();
				if (!e.MoveNext ())
					throw new GeometryError ("Geometry must contain at least one vertex");
				var result = new BBox (e.Current.Position.Xyz);
				while (e.MoveNext ())
					result += e.Current.Position.Xyz;
				return result;
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
		public static V Vertex (Vector4 pos, Vector4 col)
		{
			var vertex = new V ();
			vertex.Position = pos;
			vertex.Color = col;
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
			return new Rectangle<V> (width, height);
		}

		public static Geometry<V> Transform<V> (this Geometry<V> geometry, Matrix4 matrix) where V : struct, IVertex
		{
			return new Transform<V> (geometry, matrix);
		}

		public static Geometry<V> Material<V> (this Geometry<V> geometry, IMaterial material) where V : struct, IVertex
		{
			geometry.Material = material;
			return geometry;
		}
	}
}

namespace Visual3D
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;

	public interface IVertex
	{
		Vector4 Position { get; set; }
		Vector4 Color { get; set; }
	}

	public abstract class Geometry<V> where V : struct, IVertex
	{
		public abstract int VertexCount { get; }
		public abstract IEnumerable<V> Vertices { get; }
		public abstract IEnumerable<int> Indices { get; }

		public static V Vertex (Vector4 pos, Vector4 col)
		{
			var vertex = new V ();
			vertex.Position = pos;
			vertex.Color = col;
			return vertex;
		}
	}

	public static class Geometry
	{
		public static Geometry<V> Rectangle<V> (float width, float height, Vector4 color) where V : struct, IVertex
		{
			return new Rectangle<V> (width, height, color);
		}

		public static Geometry<V> Composite<V> (params Geometry<V>[] geometries) where V : struct, IVertex
		{
			return new Composite<V> (geometries);
		}

		public static Geometry<V> Transform<V> (this Geometry<V> geometry, Matrix4 matrix) where V : struct, IVertex
		{
			return new Transform<V> (geometry, matrix);
		}
	}
}

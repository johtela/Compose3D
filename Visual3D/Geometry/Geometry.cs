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

		public static Geometry<V> Cube<V> (float width, float height, float depth, Vector4 color) where V : struct, IVertex
		{
			var right = width / 2.0f;
			var left = -right;
			var top = height / 2.0f;
			var bottom = -top;
			var front = depth / 2.0f;
			var back = -front;
			var a180 = (float)Math.PI;
			var a90 = a180 / 2.0f;
			Func<Geometry<V>> rect = () => Rectangle<V> (width, height, color);
			
			return Composite<V> (
					rect ().Transform (Matrix4.CreateTranslation (0.0f, 0.0f, front)),
					rect ().Transform (Matrix4.CreateRotationX (a180) * Matrix4.CreateTranslation (0.0f, 0.0f, back)),
					rect ().Transform (Matrix4.CreateRotationX (-a90) * Matrix4.CreateTranslation (0.0f, top, 0.0f)),
					rect ().Transform (Matrix4.CreateRotationX (a90) * Matrix4.CreateTranslation (0.0f, bottom, 0.0f)),
					rect ().Transform (Matrix4.CreateRotationY (-a90) * Matrix4.CreateTranslation (left, 0.0f, 0.0f)),
					rect ().Transform (Matrix4.CreateRotationY (a90) * Matrix4.CreateTranslation (right, 0.0f, 0.0f))
				);
		}
	}
}

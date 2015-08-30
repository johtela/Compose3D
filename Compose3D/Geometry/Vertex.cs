﻿namespace Compose3D.Geometry
{
	using Arithmetics;
	using System;
	using System.Collections.Generic;
	using System.Linq;

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

		/// <summary>
		/// Tag can be used to identify a vertex.
		/// </summary>
		int Tag { get; set; }
	}

	public static class VertexHelpers
	{
		public static IEnumerable<V> Leftmost<V> (this IEnumerable<V> vertices)
			where V : struct, IVertex
		{
			return vertices.MinimumItems (v => v.Position.X);
		}

		public static IEnumerable<V> Rightmost<V>  (this IEnumerable<V> vertices)
			where V : struct, IVertex
		{
			return vertices.MaximumItems (v => v.Position.X);
		}

		public static IEnumerable<V> Bottommost<V>  (this IEnumerable<V> vertices)
			where V : struct, IVertex
		{
			return vertices.MinimumItems (v => v.Position.Y);
		}

		public static IEnumerable<V> Topmost<V>  (this IEnumerable<V> vertices)
			where V : struct, IVertex
		{
			return vertices.MaximumItems (v => v.Position.Y);
		}

		public static IEnumerable<V> Backmost<V>  (this IEnumerable<V> vertices)
			where V : struct, IVertex
		{
			return vertices.MinimumItems (v => v.Position.Z);
		}

		public static IEnumerable<V> Frontmost<V>  (this IEnumerable<V> vertices)
			where V : struct, IVertex
		{
			return vertices.MaximumItems (v => v.Position.Z);
		}

		public static bool AreCoplanar<V> (params V[] vertices) where V : struct, IVertex
		{
			if (vertices.Length < 4)
				return true;
			var first = vertices [0].Position;
			var ab = vertices [1].Position - first;
			var ac = vertices [2].Position - first;
			var normal = ab.Cross (ac);

			for (int i = 3; i < vertices.Length; i++)
				if (!normal.Dot (vertices [i].Position - first).ApproxEquals (0f))
					return false;
			return true;
		}
	}
}


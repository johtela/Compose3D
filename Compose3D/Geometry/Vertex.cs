namespace Compose3D.Geometry
{
	using Maths;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Extensions;

	/// <summary>
	/// Interface for objects with any position data.
	/// </summary>
	public interface IVertex<V> where V : struct, IVec<V, float>
	{
		/// <summary>
		/// Position of the object.
		/// </summary>
		V position { get; set; }
	}

    public interface IVertex2D : IVertex<Vec2> { }

	/// <summary>
	/// Interface that is used to access mandatory vertex attributes.
	/// </summary>
	/// <description>
	/// All Vertex structures need to implement this interface. Through it
	/// geometry generators can set vertex attributes.
	/// </description>
	public interface IVertex3D : IVertex<Vec3>
    {
        /// <summary>
        /// The normal of the object.
        /// </summary>
        Vec3 normal { get; set; }
    }

    public interface IVertexInitializer<V, D>
		where V : struct, IVertex<D>
        where D : struct, IVec<D, float>
	{
		void Initialize (ref V vertex);
	}

	public interface ITagged<V> where V : IVertex3D
	{
		/// <summary>
		/// Tag can be used to identify a vertex.
		/// </summary>
		int tag { get; set; }
	}

	public static class Dir3D
	{
		public static Vec3 Left = new Vec3 (-1f, 0f, 0f);
		public static Vec3 Right = new Vec3 (1f, 0f, 0f);
		public static Vec3 Down = new Vec3 (0f, -1f, 0f);
		public static Vec3 Up = new Vec3 (0f, 1f, 0f);
		public static Vec3 Back = new Vec3 (0f, 0f, -1f);
		public static Vec3 Front = new Vec3 (0f, 0f, 1f);
	}

	public static class VertexHelpers
	{
		private static int _lastTag;

		public static V New<V>(Vec3 position, Vec3 normal)
            where V : struct, IVertex3D
        {
            var vertex = new V
            {
                position = position,
                normal = normal
            };
            if (vertex is IVertexInitializer<V, Vec3>)
                (vertex as IVertexInitializer<V, Vec3>).Initialize (ref vertex);
            return vertex;
        }

		public static V With<V>(this V vertex, Vec3 position, Vec3 normal)
			where V : struct, IVertex3D
		{
			vertex.position = position;
			vertex.normal = normal;
			return vertex;
		}

		public static V Center<P, V> (this IEnumerable<P> vertices)
			where P : struct, IVertex<V>
			where V : struct, IVec<V, float>
		{
			var extents = vertices.Extents<P, V> ();
			return extents.Item1.Add (extents.Item2).Divide (2f);
		}

		public static Tuple<V, V> Extents<P, V> (this IEnumerable<P> vertices)
			where P : struct, IVertex<V>
			where V : struct, IVec<V, float>
		{
			var min = Vec.New<V, float> (float.PositiveInfinity);
			var max = Vec.New<V, float> (float.NegativeInfinity);

			foreach (var pos in vertices)
				for (int i = 0; i < min.Dimensions; i++)
				{
					if (min[i] > pos.position[i])
						min[i] = pos.position[i];
					if (max[i] < pos.position[i])
						max[i] = pos.position[i];
				}
			return Tuple.Create (min, max);
		}

		public static IEnumerable<P> Furthest<P, V> (this IEnumerable<P> vertices, V direction)
			where P : struct, IVertex<V>
			where V : struct, IVec<V, float>
		{
			return vertices.MaximumItems (v => v.position.Dot (direction));
		}
		
		public static IEnumerable<V> Facing<V> (this IEnumerable<V> vertices, Vec3 direction)
			where V : struct, IVertex3D
		{
			return vertices.Where (v => v.Facing (direction));
		}
		
		public static bool AreCoplanar<P> (this IEnumerable<P> vertices) 
			where P : struct, IVertex<Vec3>
		{
			if (vertices.Count () < 4)
				return true;
			var first = EnumerableExt.Next (ref vertices).position;
			var ab = EnumerableExt.Next (ref vertices).position - first;
			var ac = EnumerableExt.Next (ref vertices).position - first;
			var normal = ab.Cross (ac);

			return vertices.All (v => normal.Dot (v.position - first).ApproxEquals (0f, 0.1f));
		}

		public static int TagVertex<V> (this Geometry<V> geometry, V vertex) 
			where V : struct, IVertex3D, ITagged<V>
		{
			geometry.Vertices[geometry.FindVertex (vertex)].tag = ++_lastTag;
			return _lastTag;
		}

		public static V FindVertexByTag<V> (this Geometry<V> geometry, int tag)
			where V : struct, IVertex3D, ITagged<V>
		{
			return geometry.Vertices.First (v => v.tag == tag);
		}
	}
}
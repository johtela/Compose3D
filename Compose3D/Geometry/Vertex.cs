namespace Compose3D.Geometry
{
	using Maths;
    using Textures;
	using System.Collections.Generic;
	using System.Linq;
	
	/// <summary>
	/// Interface for objects with any position data.
	/// </summary>
	public interface IPositional<V> where V : struct, IVec<V, float>
	{
		/// <summary>
		/// Position of the object.
		/// </summary>
		V Position { get; set; }
	}

	/// <summary>
	/// Interface for objects with normals.
	/// </summary>
	public interface IPlanar<V> where V : struct, IVec<V, float>
	{
		/// <summary>
		/// The normal of the object.
		/// </summary>
		V Normal { get; set; }
	}

	/// <summary>
	/// Interface that is used to access mandatory vertex attributes.
	/// </summary>
	/// <description>
	/// All Vertex structures need to implement this interface. Through it
	/// geometry generators can set vertex attributes.
	/// </description>
	public interface IVertex : IPositional<Vec3>, IPlanar<Vec3>
	{
		/// <summary>
		/// Tag can be used to identify a vertex.
		/// </summary>
		int Tag { get; set; }
	}

	public interface IVertexInitializer<V>
		where V : struct, IVertex
	{
		void Initialize (ref V vertex);
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
        public static V New<V>(Vec3 position, Vec3 normal, int tag)
            where V : struct, IVertex
        {
            var vertex = new V ();
            vertex.Position = position;
            vertex.Normal = normal;
            vertex.Tag = tag;
            if (vertex is IVertexInitializer<V>)
                (vertex as IVertexInitializer<V>).Initialize (ref vertex);
            return vertex;
        }

        public static V New<V>(Vec3 position, Vec3 normal)
            where V : struct, IVertex
        {
            return New<V> (position, normal, 0);
        }

		public static V With<V>(this V vertex, Vec3 position, Vec3 normal)
			where V : struct, IVertex
		{
			vertex.Position = position;
			vertex.Normal = normal;
			return vertex;
		}

		public static IEnumerable<P> Furthest<P, V> (this IEnumerable<P> vertices, V direction)
			where P : struct, IPositional<V>
			where V : struct, IVec<V, float>
		{
			return vertices.MaximumItems (v => v.Position.Multiply (direction).Sum ());
		}
		
		public static IEnumerable<P> Facing<P, V> (this IEnumerable<P> vertices, V direction)
			where P : struct, IPlanar<V>
			where V : struct, IVec<V, float>
		{
			return vertices.Where (v => v.Normal.Dot (direction).ApproxEquals (1f, 0.1f));
		}
		
		public static bool AreCoplanar<P> (this IEnumerable<P> vertices) 
			where P : struct, IPositional<Vec3>
		{
			if (vertices.Count () < 4)
				return true;
			var first = Ext.Next (ref vertices).Position;
			var ab = Ext.Next (ref vertices).Position - first;
			var ac = Ext.Next (ref vertices).Position - first;
			var normal = ab.Cross (ac);

			return vertices.All (v => normal.Dot (v.Position - first).ApproxEquals (0f, 0.1f));
		}
	}
}
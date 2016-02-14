namespace Compose3D.Geometry
{
    using Compose3D.Maths;
    using System;
    using System.Collections.Generic;
	using System.Linq;
	using DataStructures;

	/// <summary>
	/// Enumerations representing the axes in 3D cartesian coordinate system.
	/// </summary>
	public enum Axis { X, Y, Z }

	/// <summary>
	/// Enumeration representing a set of coordinate axes.
	/// </summary>
	[Flags]
	public enum Axes { X = 1, Y = 2, Z = 4, All = 7 }

	/// <summary>
	/// Direction of the axis; towards negative or positive values.
	/// </summary>
	public enum AxisDirection : int
	{ 
		Negative = -1, 
		Positive = 1
	}

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
	public abstract class Geometry<V> : ITransformable<Geometry<V>, Mat4> 
		where V : struct, IVertex
	{
		private static int _lastTag;
		private Aabb<Vec3> _boundingBox;
		private V[] _vertices;
		private int[] _indices;
        private V[] _normals;

		protected abstract IEnumerable<V> GenerateVertices ();
		protected abstract IEnumerable<int> GenerateIndices ();

		/// <summary>
		/// Enumerates the vertices of the geometry.
		/// </summary>
		public V[] Vertices 
		{ 
			get
            {
                if (_vertices == null)
                    _vertices = GenerateVertices ().ToArray ();
                return _vertices;
            }
		}

		/// <summary>
		/// Enumerates the indices used to draw triangles of the geometry.
		/// </summary>
		public int[] Indices 
		{ 
			get
            {
                if (_indices == null)
                    _indices = GenerateIndices ().ToArray ();
                return _indices;
            }
		}

		/// <summary>
		/// Return the bounding box of this geometry.
		/// </summary>
		public virtual Aabb<Vec3> BoundingBox 
		{
			get
			{
				if (_boundingBox == null)
					_boundingBox = Aabb<Vec3>.FromPositions (Vertices.Select (v => v.Position));
				return _boundingBox;
			}
		}

        private IEnumerable<V> GenerateNormals ()
        {
            foreach (var v in Vertices)
            {
                yield return VertexHelpers.New<V> (v.Position, v.Normal);
                yield return VertexHelpers.New<V> (v.Position + v.Normal, v.Normal);
            }
        }

        public V[] Normals
		{
			get
			{
                if (_normals == null)
                    _normals = GenerateNormals ().ToArray ();
                return _normals;
			}
		}

		public int FindVertex (V vertex)
		{
			for (int i = 0; i < Vertices.Length; i++)
				if (Vertices [i].Equals (vertex))
					return i;
			throw new ArgumentException ("Could not find vertex: " + vertex);
		}

		public int TagVertex (V vertex)
		{
			Vertices [FindVertex (vertex)].Tag = ++_lastTag;
			return _lastTag;
		}

		public V FindVertexByTag (int tag)
		{
			return Vertices.First (v => v.Tag == tag);
		}

		#region ITransformable implementation

		public Geometry<V> Transform (Mat4 matrix)
		{
			if (this is Transform<V>)
			{
				var trans = this as Transform<V>;
				return new Transform<V> (trans._geometry, matrix * trans._matrix);
			}
			else
				return new Transform<V> (this, matrix);
		}
		
		public Geometry<V> ReverseWinding ()
		{
			return new ReverseIndices<V> (this);			
		}

		#endregion
	}

	/// <summary>
	/// Contains static and extension methods that can be used compose geometries.
	/// </summary>
	public static class Geometry
	{
		public static Geometry<V> Center<V> (this Geometry<V> geometry) where V : struct, IVertex
		{
			var center = geometry.BoundingBox.Center;
			return geometry.Translate (-center.X, -center.Y, -center.Z);
		}

		private static Vec3 GetSnapOffset (Vec3 pos, Vec3 snapToPos, Axes snapAxes)
		{
			var result = snapToPos - pos;
			if ((snapAxes & Axes.X) == 0)
				result.X = 0f;
			if ((snapAxes & Axes.Y) == 0)
				result.Y = 0f;
			if ((snapAxes & Axes.Z) == 0)
				result.Z = 0f;
			return result;
		}

		public static Geometry<V> SnapVertex<V> (this Geometry<V> geometry, Vec3 position, V snapToVertex, Axes snapAxes)
			where V : struct, IVertex
		{
			var offset = GetSnapOffset (position, snapToVertex.Position, snapAxes);
			return geometry.Translate (offset.X, offset.Y, offset.Z);
		}

		public static Geometry<V> SnapVertex<V> (this Geometry<V> geometry, V vertex, V snapToVertex, Axes snapAxes)
			where V : struct, IVertex
		{
			return geometry.SnapVertex (vertex.Position, snapToVertex, snapAxes);
		}
	}
}

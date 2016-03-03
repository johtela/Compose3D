namespace Compose3D.SceneGraph
{
	using System;
    using Maths;
    using Geometry;
    using GLTypes;
    using OpenTK.Graphics.OpenGL;
	using DataStructures;

	public class TerrainMesh<V> : SceneNode
		where V : struct, IVertex
	{
		private VBO<V> _vertexBuffer;
		private VBO<int> _indexBuffer;
		private Aabb<Vec3> _boundingBox;

		public TerrainMesh (SceneGraph graph, Vec2i start, Vec2i size)
			: base (graph)
		{
			Patch = new TerrainPatch<V> (start, size);
		}

		public TerrainPatch<V> Patch { get; private set; }

		public VBO<V> VertexBuffer
		{
			get
			{
				if (_vertexBuffer == null)
					_vertexBuffer = new VBO<V> (Patch.Vertices, BufferTarget.ArrayBuffer);
				return _vertexBuffer;
			}
		}

		public VBO<int> IndexBuffer
		{
			get
			{
				if (_indexBuffer == null)
					_indexBuffer = new VBO<int> (Patch.Indices, BufferTarget.ElementArrayBuffer);
				return _indexBuffer;
			}
		}

		internal override void RemoveFromIndex ()
		{
			throw new InvalidOperationException ("Cannot remove a terrain mesh from index.");
		}

		public override Aabb<Vec3> BoundingBox
		{
			get
			{
				if (_boundingBox == null)
					_boundingBox = Transform * Patch.BoundingBox;
				return _boundingBox;
			}
		}
	}
}

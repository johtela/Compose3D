﻿namespace Compose3D.SceneGraph
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
		private VBO<int>[] _indexBuffers;
		private Aabb<Vec3> _boundingBox;
		private Vec2i _start;
		private Vec2i _size;
		private TerrainPatch<V> _patch;

		public TerrainMesh (SceneGraph graph, Vec2i start, Vec2i size)
			: base (graph)
		{
			_start = start;
			_size = size;
		}

		public TerrainPatch<V> Patch
		{
			get
			{
				if (_patch == null)
					_patch = new TerrainPatch<V> (_start, _size, 20f);
				return _patch;
			}
		}

		public VBO<V> VertexBuffer
		{
			get
			{
				if (_vertexBuffer == null && Patch.Vertices != null)
					_vertexBuffer = new VBO<V> (Patch.Vertices, BufferTarget.ArrayBuffer);
				return _vertexBuffer;
			}
		}

		public VBO<int>[] IndexBuffers
		{
			get
			{
				if (_indexBuffers == null && Patch.Indices != null)
				{
					var len = Patch.Indices.Length;
					_indexBuffers = new VBO<int>[len];
					for (int i = 0; i < len; i++)
						_indexBuffers[i] = new VBO<int> (Patch.Indices[i], BufferTarget.ElementArrayBuffer);
				}
				return _indexBuffers;
			}
		}

		internal override void RemoveFromIndex ()
		{
			if (Root == Graph.Root)
				throw new InvalidOperationException ("Cannot remove a terrain mesh from index. It is a static node.");
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

namespace Compose3D.SceneGraph
{
	using System;
	using DataStructures;
	using Geometry;
	using Maths;
	using Textures;

	public class StaticMesh<V> : Mesh<V>
        where V : struct, IVertex
	{
		private Mat4? _transform;
		private Aabb<Vec3> _boundingBox;

		public StaticMesh (SceneGraph graph, Geometry<V> geometry, params Texture[] textures)
			: base (graph, geometry, textures) { }

		public override Mat4 Transform
		{
			get
			{
				if (!_transform.HasValue)
					_transform = base.Transform;
				return _transform.Value;
			}
		}

		public override Aabb<Vec3> BoundingBox
		{
			get
			{
				if (_boundingBox == null)
					_boundingBox = base.BoundingBox;
				return base.BoundingBox;
			}
		}

		internal override void RemoveFromIndex ()
		{
			if (Root == Graph.Root)
				throw new InvalidOperationException ("Cannot remove a static mesh from index.");
		}
	}
}
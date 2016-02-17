namespace Compose3D.SceneGraph
{
	using System.Collections.Generic;
	using DataStructures;
	using Maths;

	public abstract class SceneNodeWrapper : SceneNode
	{
		public readonly SceneNode Node;

		public SceneNodeWrapper (SceneNode node)
		{
			Node = node;
		}

		public override Aabb<Vec3> BoundingBox
		{
			get { return Node.BoundingBox; }
		}
	}
}
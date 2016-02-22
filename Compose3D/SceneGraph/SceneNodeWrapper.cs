namespace Compose3D.SceneGraph
{
	using System.Collections.Generic;
	using DataStructures;
	using Maths;
	using Extensions;

	public abstract class SceneNodeWrapper : SceneNode
	{
		public readonly SceneNode Node;

		public SceneNodeWrapper (SceneGraph graph, SceneNode node) : base (graph)
		{
			Node = node;
			Node.Parent = this;
		}

		public override IEnumerable<SceneNode> Traverse ()
		{
			return Node.Traverse ().Append (this);
		}

		public override Aabb<Vec3> BoundingBox
		{
			get { return Node.BoundingBox; }
		}
	}
}
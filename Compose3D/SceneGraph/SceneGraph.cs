namespace Compose3D.SceneGraph
{
	using System;
	using DataStructures;
	using Maths;

	public class SceneGraph
	{
		public SceneGraph ()
		{
			Root = new SceneGroup (this);
			Index = new BoundingBoxTree<SceneNode> ();
		}

		public IBoundingTree<Vec3, SceneNode> Index { get; internal set; }
		public SceneGroup Root { get; private set; }

		private void UpdateIndex (SceneNode node, bool add)
		{
			foreach (var subNode in node.Traverse ())
			{
				var bbox = subNode.BoundingBox;
				if (bbox != null)
				{
					if (add)
						Index.Add (subNode.BoundingBox, subNode);
					else
						Index.Remove (subNode.BoundingBox, subNode);
				}
			}
		}

		internal void AddToIndex (SceneNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			UpdateIndex (node, true);
		}

		internal void RemoveFromIndex (SceneNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			UpdateIndex (node, false);
		}
	}
}

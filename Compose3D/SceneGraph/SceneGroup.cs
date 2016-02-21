namespace Compose3D.SceneGraph
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Maths;
	using DataStructures;
	using Extensions;

	public class SceneGroup : SceneNode
	{
		private List<SceneNode> _subNodes;

		public SceneGroup () 
		{
			_subNodes = new List<SceneNode> ();
		}

		public SceneGroup (IEnumerable<SceneNode> subNodes) : this ()
		{
			Add (subNodes);
		}

		public SceneGroup (params SceneNode[] subNodes) : this ()
		{
			Add (subNodes);
		}

		public virtual SceneNode Add (IEnumerable<SceneNode> subNodes)
		{
			_subNodes.AddRange (subNodes);
			foreach (var subNode in subNodes)
				subNode.Parent = this;
			return this;
		}

		public override IEnumerable<SceneNode> Traverse ()
		{
			return _subNodes.SelectMany (sn => sn.Traverse ()).Append (this);
		}

		public override Aabb<Vec3> BoundingBox
		{
			get { return null; }
		}

		public IEnumerable<SceneNode> SubNodes
		{
			get { return _subNodes; }
		}
	}
}

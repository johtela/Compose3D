namespace Compose3D.SceneGraph
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Maths;
	using DataStructures;

	public class SceneGroup : SceneNode
	{
		private List<SceneNode> _subNodes;

		public SceneGroup () { }

		public SceneGroup (IEnumerable<SceneNode> subNodes)
		{
			Add (subNodes);
		}

		public SceneGroup (params SceneNode[] subNodes)
		{
			Add (subNodes);
		}

		public virtual SceneNode Add (IEnumerable<SceneNode> subNodes)
		{
			if (_subNodes == null)
				_subNodes = new List<SceneNode> (subNodes);
			else
				_subNodes.AddRange (subNodes);
			return this;
		}

		public override IEnumerable<Tuple<SceneNode, Mat4>> Traverse (Mat4 transform)
		{
			return SubNodes.SelectMany (sn => sn.Traverse (transform)).Concat (base.Traverse (transform));
		}

		public override Aabb<Vec3> BoundingBox
		{
			get
			{
				return _subNodes == null ? null :
					(from node in _subNodes
					 where node != null
					 select node.BoundingBox).Aggregate ((b1, b2) => b1 + b2);
			}
		}

		public IEnumerable<SceneNode> SubNodes
		{
			get { return _subNodes == null ? Enumerable.Empty<SceneNode> () : _subNodes; }
		}
	}
}

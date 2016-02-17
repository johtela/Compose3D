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

		public override void Traverse<T> (Action<T, Mat4> action, Mat4 transform)
		{
			if (_subNodes != null)
				foreach (var subNode in _subNodes)
					subNode.Traverse<T> (action, transform);
			base.Traverse<T> (action, transform);
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

		public override IEnumerable<SceneNode> Descendants
		{
			get
			{
				return _subNodes != null ?
					_subNodes.SelectMany (sn => sn.Descendants) :
					Enumerable.Empty<SceneNode> ();
			}
		}

		public IEnumerable<SceneNode> SubNodes
		{
			get { return _subNodes == null ? Enumerable.Empty<SceneNode> () : _subNodes; }
		}
	}
}

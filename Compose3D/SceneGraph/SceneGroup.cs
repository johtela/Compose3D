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

		public SceneGroup (SceneGraph graph) : base (graph)
		{
			_subNodes = new List<SceneNode> ();
		}

		public SceneGroup (SceneGraph graph, IEnumerable<SceneNode> subNodes) : this (graph)
		{
			Add (subNodes);
		}

		public SceneGroup (SceneGraph graph, params SceneNode[] subNodes) : this (graph)
		{
			Add (subNodes);
		}

		public SceneNode Add (IEnumerable<SceneNode> subNodes)
		{
			_subNodes.AddRange (subNodes);
			foreach (var subNode in subNodes)
			{
				subNode.Parent = this;
				foreach (var node in subNode.Traverse ())
					node.AddToIndex ();
			}
			return this;
		}

		public override IEnumerable<SceneNode> Traverse ()
		{
			return _subNodes.SelectMany (sn => sn.Traverse ()).Append (this);
		}

		public IEnumerable<SceneNode> SubNodes
		{
			get { return _subNodes; }
		}
	}
}

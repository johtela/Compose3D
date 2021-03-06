﻿namespace ComposeFX.Graphics.SceneGraph
{
	using System.Collections.Generic;
	using Extensions;

	public abstract class NodeWrapper<T> : SceneNode
		where T : SceneNode
	{
		public readonly T Node;

		public NodeWrapper (SceneGraph graph, T node) : base (graph)
		{
			Node = node;
			Node.Parent = this;
		}

		public override IEnumerable<SceneNode> Traverse ()
		{
			return Node.Traverse ().Append (this);
		}
	}
}
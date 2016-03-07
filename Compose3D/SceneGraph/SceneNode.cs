namespace Compose3D.SceneGraph
{
    using System;
    using System.Collections.Generic;
	using System.Linq;
	using Maths;
	using DataStructures;
	using Extensions;

	public abstract class SceneNode
    {
		private SceneNode _parent;

		public SceneNode (SceneGraph graph)
		{
			if (graph == null)
				throw new ArgumentNullException ("graph");
			Graph = graph;
		}

		public SceneGraph Graph { get; private set; }

		public virtual Aabb<Vec3> BoundingBox
		{
			get { return null; }
		}

		public SceneNode Parent
		{
			get { return _parent; }
			internal set
			{
				if (_parent != null && _parent.Graph != Graph)
					throw new InvalidOperationException ("Cannot add nodes belonging to another scene graph.");
				_parent = value;
			}
		}

		public SceneNode Root
		{
			get { return Ancestors.LastOrDefault () ?? this; }
		}

		public IEnumerable<SceneNode> Ancestors
		{
			get
			{
				var node = Parent;
				while (node != null)
				{
					yield return node;
					node = node.Parent;
				}
			}
		}

		public virtual Mat4 Transform
		{
			get { return Parent == null ? new Mat4 (1f) : Parent.Transform; }
		}

		public virtual IEnumerable<SceneNode> Traverse () 
        {
			return EnumerableExt.Enumerate (this); 
        }

		internal virtual void AddToIndex ()
		{
			if (Root != Graph.Root)
				return;
			var bbox = BoundingBox;
			if (bbox != null)
				Graph.Index.Add (bbox, this);
		}

		internal virtual void RemoveFromIndex ()
		{
			if (Root != Graph.Root)
				return;
			var bbox = BoundingBox;
			if (bbox != null)
				Graph.Index.Remove (bbox, this);
		}
	}
}
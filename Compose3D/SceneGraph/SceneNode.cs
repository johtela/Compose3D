namespace Compose3D.SceneGraph
{
    using System;
    using System.Collections.Generic;
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
			get
			{
				var result = this;
				while (result.Parent != null)
					result = result.Parent;
				return result;
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
	}
}
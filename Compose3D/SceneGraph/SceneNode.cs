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
		public SceneNode Parent { get; internal set; }

		public abstract Aabb<Vec3> BoundingBox { get; }

		public virtual Mat4 Transform
		{
			get { return Parent == null ? new Mat4 (1f) : Parent.Transform; }
		}

		public virtual IEnumerable<SceneNode> Traverse () 
        {
			return EnumerableExt.Enumerate (this); 
        }

		public virtual IEnumerable<SceneNode> OverlapWith (Aabb<Vec3> bbox)
		{
			return bbox & BoundingBox ? EnumerableExt.Enumerate (this) : Enumerable.Empty<SceneNode> ();
		}
	}
}
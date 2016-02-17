namespace Compose3D.SceneGraph
{
    using System;
    using System.Collections.Generic;
	using System.Linq;
	using Maths;
	using DataStructures;

	public abstract class SceneNode
    {
		public virtual IEnumerable<SceneNode> Descendants
		{
			get { return Enumerable.Empty<SceneNode> (); }
		}

		public abstract Aabb<Vec3> BoundingBox { get; }

		public virtual void Traverse<T> (Action<T, Mat4> action, Mat4 transform) 
			where T : SceneNode
        {
            if (this is T)
				action ((T)this, transform);
        }

		public virtual IEnumerable<SceneNode> OverlapWith (Aabb<Vec3> bbox)
		{
			return bbox & BoundingBox ? Ext.Enumerate (this) : Enumerable.Empty<SceneNode> ();
		}
	}
}
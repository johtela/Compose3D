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
		public abstract Aabb<Vec3> BoundingBox { get; }

		public virtual IEnumerable<Tuple<SceneNode, Mat4>> Traverse (Mat4 transform) 
        {
			return EnumerableExt.Enumerate (Tuple.Create (this, transform)); 
        }

		public virtual IEnumerable<SceneNode> OverlapWith (Aabb<Vec3> bbox)
		{
			return bbox & BoundingBox ? EnumerableExt.Enumerate (this) : Enumerable.Empty<SceneNode> ();
		}
	}
}
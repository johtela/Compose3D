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

		public virtual IEnumerable<Tuple<SceneNode, Mat4>> Traverse (Func<SceneNode, Mat4, bool> predicate,
			Mat4 transform) 
        {
			var current = Tuple.Create (this, transform);
			return predicate (current.Item1, current.Item2) ? 
				EnumerableExt.Enumerate (current) : 
				Enumerable.Empty<Tuple<SceneNode, Mat4>> ();
        }

		public virtual IEnumerable<SceneNode> OverlapWith (Aabb<Vec3> bbox)
		{
			return bbox & BoundingBox ? EnumerableExt.Enumerate (this) : Enumerable.Empty<SceneNode> ();
		}
	}
}
namespace Compose3D.SceneGraph
{
    using Compose3D.Maths;
    using Geometry;
    using System;
    using System.Collections.Generic;
	using System.Linq;
	using Extensions;

    public static class Scene
    {
        public static SceneNode Add (this SceneGroup group, params SceneNode[] subNodes)
        {
            return group.Add (subNodes as IEnumerable<SceneNode>);
        }

		public static SceneNode OffsetOrientAndScale (this SceneNode node, Vec3 offset, Vec3 orientation, Vec3 scale)
		{
			return new TransformNode (node.Graph, node, offset, orientation, scale);
		}

		public static SceneNode Offset (this SceneNode node, Vec3 offset)
		{
			return OffsetOrientAndScale (node, offset, new Vec3 (0f), new Vec3 (1f));
		}

		public static SceneNode Orient (this SceneNode node, Vec3 orientation)
		{
			return OffsetOrientAndScale (node, new Vec3 (0f), orientation, new Vec3 (1f));
		}

		public static SceneNode Scale (this SceneNode node, Vec3 factor)
		{
			return OffsetOrientAndScale (node, new Vec3 (0f), factor, new Vec3 (1f));
		}
	}
}
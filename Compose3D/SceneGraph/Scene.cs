namespace Compose3D.SceneGraph
{
    using Arithmetics;
    using Geometry;
    using System;
    using System.Collections.Generic;

    public static class Scene
    {
        public static SceneNode Add (this SceneNode node, IEnumerable<SceneNode> subNodes)
        {
            foreach (var subNode in subNodes)
                node.SubNodes.Add (subNode);
            return node;
        }

        public static SceneNode Add (this SceneNode node, params SceneNode[] subNodes)
        {
            return Add (node, subNodes as IEnumerable<SceneNode>);
        }

		public static SceneNode Transform (this SceneNode node, Mat4 matrix)
		{
			return new Transformation (matrix).Add (node);
		}

		public static SceneNode OffsetOrientAndScale (this SceneNode node, Vec3 offset, Vec3 orientation, Vec3 scale)
		{
			return new OffsetOrientationScale (offset, orientation, scale).Add (node);
		}

		public static SceneNode Offset (this SceneNode node, Vec3 offset)
		{
			return OffsetOrientAndScale (node, offset, new Vec3 (0f), new Vec3 (1f));
		}

		public static SceneNode Orient (this SceneNode node, Vec3 orientation)
		{
			return OffsetOrientAndScale (node, new Vec3 (0f), orientation, new Vec3 (1f));
		}

		public static SceneNode Scale (this SceneNode node, Vec3 orientation)
		{
			return OffsetOrientAndScale (node, new Vec3 (0f), orientation, new Vec3 (1f));
		}
	}
}

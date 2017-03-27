namespace Compose3D.SceneGraph
{
    using Maths;
    using System.Collections.Generic;

	public static class Scene
    {
        public static SceneNode Add (this SceneGroup group, params SceneNode[] subNodes)
        {
            return group.Add (subNodes as IEnumerable<SceneNode>);
        }

		public static TransformNode<T> OffsetOrientAndScale<T> (this T node, Vec3 offset,
			Vec3 orientation, Vec3 scale)
			where T : SceneNode
		{
			return new TransformNode<T> (node.Graph, node, offset, orientation, scale);
		}

		public static TransformNode<T> Offset<T> (this T node, Vec3 offset)
			where T : SceneNode
		{
			return OffsetOrientAndScale (node, offset, new Vec3 (0f), new Vec3 (1f));
		}

		public static TransformNode<T> Orient<T> (this T node, Vec3 orientation)
			where T : SceneNode
		{
			return OffsetOrientAndScale (node, new Vec3 (0f), orientation, new Vec3 (1f));
		}

		public static TransformNode<T> Scale<T> (this T node, Vec3 factor)
			where T : SceneNode
		{
			return OffsetOrientAndScale (node, new Vec3 (0f), new Vec3 (0f), factor);
		}
	}
}
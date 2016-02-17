namespace Compose3D.SceneGraph
{
    using Compose3D.Maths;
    using Geometry;
    using System;
    using System.Collections.Generic;

    public static class Scene
    {
        public static SceneNode Add (this SceneGroup group, params SceneNode[] subNodes)
        {
            return group.Add (subNodes as IEnumerable<SceneNode>);
        }

		public static SceneNode OffsetOrientAndScale (this SceneNode node, Vec3 offset, Vec3 orientation, Vec3 scale)
		{
			return new TransformNode (node, offset, orientation, scale);
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

		public static void Traverse<T> (this SceneNode sceneNode, Action<T, Mat4> action) where T : SceneNode
		{
			sceneNode.Traverse (action, new Mat4 (1f));
		}

		public static void Traverse<T, U> (this SceneNode sceneNode, Action<T, Mat4> actionT, Action<U, Mat4> actionU, 
			Mat4 transform)
			where T : SceneNode
			where U : SceneNode
		{
			sceneNode.Traverse<SceneNode> ((node, mat) =>
			{
				if (node is T)
					actionT ((T)node, mat);
				else if (node is U)
					actionU ((U)node, mat);
			}, transform);
		}

		public static void Traverse<T, U> (this SceneNode sceneNode, Action<T, Mat4> actionT, Action<U, Mat4> actionU)
			where T : SceneNode
			where U : SceneNode
		{
			sceneNode.Traverse (actionT, actionU, new Mat4 (1f));
		}

		public static void Traverse<T, U, V> (this SceneNode sceneNode, Action<T, Mat4> actionT, Action<U, Mat4> actionU, 
			Action<V, Mat4> actionV, Mat4 transform)
			where T : SceneNode
			where U : SceneNode
			where V : SceneNode
		{
			sceneNode.Traverse<SceneNode> ((node, mat) =>
			{
				if (node is T)
					actionT ((T)node, mat);
				else if (node is U)
					actionU ((U)node, mat);
				else if (node is V)
					actionV ((V)node, mat);
			}, transform);
		}

		public static void Traverse<T, U, V> (this SceneNode sceneNode, Action<T, Mat4> actionT, Action<U, Mat4> actionU, 
			Action<V, Mat4> actionV)
			where T : SceneNode
			where U : SceneNode
			where V : SceneNode
		{
			sceneNode.Traverse (actionT, actionU, actionV, new Mat4 (1f));
		}

		public static void Traverse<T, U, V, W> (this SceneNode sceneNode, Action<T, Mat4> actionT, Action<U, Mat4> actionU, 
			Action<V, Mat4> actionV, Action<W, Mat4> actionW, Mat4 transform)
			where T : SceneNode
			where U : SceneNode
			where V : SceneNode
			where W : SceneNode
		{
			sceneNode.Traverse<SceneNode> ((node, mat) =>
			{
				if (node is T)
					actionT ((T)node, mat);
				else if (node is U)
					actionU ((U)node, mat);
				else if (node is V)
					actionV ((V)node, mat);
				else if (node is W)
					actionW ((W)node, mat);
			}, transform);
		}

		public static void Traverse<T, U, V, W> (this SceneNode sceneNode, Action<T, Mat4> actionT, Action<U, Mat4> actionU, 
			Action<V, Mat4> actionV, Action<W, Mat4> actionW)
			where T : SceneNode
			where U : SceneNode
			where V : SceneNode
			where W : SceneNode
		{
			sceneNode.Traverse (actionT, actionU, actionV, actionW, new Mat4 (1f));
		}
	}
}
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
    }
}

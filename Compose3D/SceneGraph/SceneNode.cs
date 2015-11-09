namespace Compose3D.SceneGraph
{
    using Compose3D.Maths;
    using Geometry;
    using System;
    using System.Collections.Generic;

    public abstract class SceneNode
    {
        private List<SceneNode> _subNodes;

        public ICollection<SceneNode> SubNodes
        {
            get
            {
                if (_subNodes == null)
                    _subNodes = new List<SceneNode> ();
                return _subNodes;
            }
        }

		public virtual void Traverse<T> (Action<T, Mat4, Mat3> action, Mat4 transform, Mat3 normalTransform) 
			where T : SceneNode
        {
            if (_subNodes != null)
                foreach (var subNode in _subNodes)
					subNode.Traverse<T> (action, transform, normalTransform);
            if (this is T)
				action ((T)this, transform, normalTransform);
        }

		public void Traverse<T> (Action<T, Mat4, Mat3> action) where T : SceneNode
        {
			Traverse<T> (action, new Mat4 (1f), new Mat3 (1f));
        }

		public void Traverse<T, U> (Action<T, Mat4, Mat3> actionT, Action<U, Mat4, Mat3> actionU) 
            where T : SceneNode
            where U : SceneNode
        {
			Traverse<SceneNode> ((node, mat, nmat) =>
            {
                if (node is T)
					actionT ((T)node, mat, nmat);
                else if (node is U)
					actionU ((U)node, mat, nmat);
            });
        }

		public void Traverse<T, U, V> (Action<T, Mat4, Mat3> actionT, Action<U, Mat4, Mat3> actionU, 
			Action<V, Mat4, Mat3> actionV)
            where T : SceneNode
            where U : SceneNode
            where V : SceneNode
        {
			Traverse<SceneNode> ((node, mat, nmat) =>
            {
                if (node is T)
					actionT ((T)node, mat, nmat);
                else if (node is U)
					actionU ((U)node, mat, nmat);
                else if (node is V)
					actionV ((V)node, mat, nmat);
            });
        }

		public void Traverse<T, U, V, W> (Action<T, Mat4, Mat3> actionT, Action<U, Mat4, Mat3> actionU, 
			Action<V, Mat4, Mat3> actionV, Action<W, Mat4, Mat3> actionW)
            where T : SceneNode
            where U : SceneNode
            where V : SceneNode
            where W : SceneNode
        {
			Traverse<SceneNode> ((node, mat, nmat) =>
            {
                if (node is T)
					actionT ((T)node, mat, nmat);
                else if (node is U)
					actionU ((U)node, mat, nmat);
                else if (node is V)
					actionV ((V)node, mat, nmat);
                else if (node is W)
					actionW ((W)node, mat, nmat);
            });
        }
    }
}
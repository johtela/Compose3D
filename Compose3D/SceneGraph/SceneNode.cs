namespace Compose3D.SceneGraph
{
    using Compose3D.Maths;
    using Geometry;
    using System;
    using System.Collections.Generic;
	using System.Linq;

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
		
		public IEnumerable<SceneNode> Descendants
		{
			get
			{
				return _subNodes != null ?
					_subNodes.SelectMany (sn => sn.Descendants).Append (this) :
					Ext.Enumerate (this);
			}
		}

		public virtual void Traverse<T> (Action<T, Mat4> action, Mat4 transform) 
			where T : SceneNode
        {
            if (_subNodes != null)
                foreach (var subNode in _subNodes)
					subNode.Traverse<T> (action, transform);
            if (this is T)
				action ((T)this, transform);
        }

		public void Traverse<T> (Action<T, Mat4> action) where T : SceneNode
        {
			Traverse<T> (action, new Mat4 (1f));
        }

		public void Traverse<T, U> (Action<T, Mat4> actionT, Action<U, Mat4> actionU, Mat4 transform) 
            where T : SceneNode
            where U : SceneNode
        {
			Traverse<SceneNode> ((node, mat) =>
            {
                if (node is T)
					actionT ((T)node, mat);
                else if (node is U)
					actionU ((U)node, mat);
			}, transform);
        }
		
		public void Traverse<T, U> (Action<T, Mat4> actionT, Action<U, Mat4> actionU) 
			where T : SceneNode
			where U : SceneNode
		{
			Traverse<T, U> (actionT, actionU, new Mat4 (1f));
		}

		public void Traverse<T, U, V> (Action<T, Mat4> actionT, Action<U, Mat4> actionU, Action<V, Mat4> actionV,
			Mat4 transform)
            where T : SceneNode
            where U : SceneNode
            where V : SceneNode
        {
			Traverse<SceneNode> ((node, mat) =>
            {
                if (node is T)
					actionT ((T)node, mat);
                else if (node is U)
					actionU ((U)node, mat);
                else if (node is V)
					actionV ((V)node, mat);
            }, transform);
        }

		public void Traverse<T, U, V> (Action<T, Mat4> actionT, Action<U, Mat4> actionU, Action<V, Mat4> actionV)
			where T : SceneNode
			where U : SceneNode
			where V : SceneNode
		{
			Traverse<T, U, V> (actionT, actionU, actionV, new Mat4 (1f));
		}
		
		public void Traverse<T, U, V, W> (Action<T, Mat4> actionT, Action<U, Mat4> actionU, Action<V, Mat4> actionV, 
			Action<W, Mat4> actionW, Mat4 transform)
            where T : SceneNode
            where U : SceneNode
            where V : SceneNode
            where W : SceneNode
        {
			Traverse<SceneNode> ((node, mat) =>
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
 
		public void Traverse<T, U, V, W> (Action<T, Mat4> actionT, Action<U, Mat4> actionU, Action<V, Mat4> actionV,
			Action<W, Mat4> actionW)
			where T : SceneNode
			where U : SceneNode
			where V : SceneNode
			where W : SceneNode
		{
			Traverse<T, U, V, W> (actionT, actionU, actionV, actionW, new Mat4 (1f));
		}
	}
}
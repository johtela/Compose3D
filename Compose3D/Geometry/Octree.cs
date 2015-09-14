namespace Compose3D.Geometry
{
    using System;
    using Arithmetics;
    using System.Collections.Generic;
    using System.Linq;

	public class Octree<P, T> where P : IPositional
    {
        private class Node
        {
			public readonly Node[] Children;
			public readonly P Positional;
			public readonly T Data;

			public Node (P positional, T data)
            {
				Children = new Node[1 << positional.Position.Dimensions];
				Positional = positional;
				Data = data;
            }
        }

		private Node _root;

		private int ChooseChild (Node node, P positional)
		{
			var result = 0;
			for (int i = 0; i < positional.Position.Dimensions; i++)
				result |= (positional.Position [i] >= node.Positional.Position [i] ? 1 : 0) << i;
			return result;
		}

		private Node FindParentNode (Node node, P positional, out int index)
		{
			index = ChooseChild (node, positional);
			var child = node.Children [index]; 
			return child == null ? node : FindParentNode (child, positional, out index);
		}

		private Node FindNode (Node node, P positional)
		{
			if (node.Positional.Equals (positional))
				return node;
			var pos = ChooseChild (node, positional);
			var child = node.Children [pos]; 
			return child == null ? null : FindNode (child, positional);
		}

		public bool Add (P positional, T data)
		{
			if (_root == null)
				_root = new Node (positional, data);
			else
			{
				int pos;
				var parent = FindParentNode (_root, positional, out pos);
				if (parent.Positional.Equals (positional))
					return false;
				parent.Children [pos] = new Node (positional, data);
			}
			return true;
		}

		public bool ContainsKey (P positional)
		{
			return FindNode (_root, positional) != null;
		}

		public bool TryGetValue (P vertex, out T value)
		{
			var node = FindNode (_root, vertex);
			var result = node != null;
			value = result ? node.Data : default (T);
			return result;
		}

		public T this[P positional]
		{
			get 
			{
				var node = FindNode (_root, positional);
				if (node == null)
					throw new KeyNotFoundException ("Positional not found");
				return node.Data;
			}
		}
    }
}

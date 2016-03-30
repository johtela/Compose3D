namespace Compose3D.DataStructures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
	using Maths;
	using Geometry;

	public class Octree<P, T, V> 
		where P : IPositional<V>
		where V : struct, IVec<V, float>
    {
        private class Node
        {
			public readonly Node[] Children;
			public readonly P Positional;
			public T Data;

			public Node (P positional, T data)
            {
				Children = new Node[1 << positional.position.Dimensions];
				Positional = positional;
				Data = data;
            }
        }

		private Node _root;

		private int ChooseChild (Node node, V position)
		{
			var result = 0;
			for (int i = 0; i < position.Dimensions; i++)
				result |= (position [i] >= node.Positional.position [i] ? 1 : 0) << i;
			return result;
		}

		private Node FindParentNode (Node node, P positional, out int index)
		{
			index = ChooseChild (node, positional.position);
			var child = node.Children [index]; 
			return child == null ? node : FindParentNode (child, positional, out index);
		}

		private Node FindNode (Node node, P positional)
		{
			if (node.Positional.Equals (positional))
				return node;
			var child = node.Children [ChooseChild (node, positional.position)]; 
			return child == null ? null : FindNode (child, positional);
		}
		
		private IEnumerable<Node> FindNodesWithPosition (Node node, V position)
		{
			if (Vec.ApproxEquals (node.Positional.position, position))
				yield return node;
			var child = node.Children [ChooseChild (node, position)]; 
			if (child != null)
				foreach (var childNode in FindNodesWithPosition (child, position))
					yield return childNode;
		}

		private Node GetNode (P positional)
		{
			var node = FindNode (_root, positional);
			if (node == null)
				throw new KeyNotFoundException ("Positional not found");
			return node;
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
		
		public IEnumerable<Tuple<P, T>> FindByPosition (V position)
		{
			return from node in FindNodesWithPosition (_root, position)
			       select Tuple.Create (node.Positional, node.Data);
		}

		public T this[P positional]
		{
			get { return GetNode (positional).Data; }
			set { GetNode (positional).Data = value; }
		}
    }
}

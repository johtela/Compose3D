namespace Compose3D.Geometry
{
    using System;
    using Arithmetics;
    using System.Collections.Generic;
    using System.Linq;

	public class Octree<V, T> 
		where V : struct, IVertex
    {
        private class Node
        {
			public readonly Node[] Children;
			public readonly V Vertex;
			public readonly T Data;

			public Node (V vertex, T data)
            {
				Children = new Node[1 << vertex.Position.Dimensions];
				Vertex = vertex;
				Data = data;
            }
        }

		private Node _root;

		private int ChooseChild (Node node, V vertex)
		{
			var result = 0;
			for (int i = 0; i < vertex.Position.Dimensions; i++)
				result |= (vertex.Position [i] >= node.Vertex.Position [i] ? 1 : 0) << i;
			return result;
		}

		private Node FindParentNode (Node node, V vertex, out int pos)
		{
			pos = ChooseChild (node, vertex);
			var child = node.Children [pos]; 
			return child == null ? node : FindParentNode (child, vertex, out pos);
		}

		private Node FindNode (Node node, V vertex)
		{
			if (node.Vertex.Equals (vertex))
				return node;
			var pos = ChooseChild (node, vertex);
			var child = node.Children [pos]; 
			return child == null ? null : FindNode (child, vertex);
		}

		public bool Add (V vertex, T data)
		{
			if (_root == null)
				_root = new Node (vertex, data);
			else
			{
				int pos;
				var parent = FindParentNode (_root, vertex, out pos);
				if (parent.Vertex.Equals (vertex))
					return false;
				parent.Children [pos] = new Node (vertex, data);
			}
			return true;
		}

		public bool ContainsKey (V vertex)
		{
			return FindNode (_root, vertex) != null;
		}

		public bool TryGetValue (V vertex, out T value)
		{
			var node = FindNode (_root, vertex);
			var result = node != null;
			value = result ? node.Data : default (T);
			return result;
		}

		public T this[V vertex]
		{
			get 
			{
				var node = FindNode (_root, vertex);
				if (node == null)
					throw new KeyNotFoundException ("Vertex not found");
				return node.Data;
			}
		}
    }
}

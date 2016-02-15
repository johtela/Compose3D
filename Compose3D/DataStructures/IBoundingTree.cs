namespace Compose3D.DataStructures
{
	using System.Collections.Generic;
	using Maths;

	public interface IBoundingTree<V, T> : IEnumerable<KeyValuePair<Aabb<V>, T>>
		where V : struct, IVec<V, float>
	{
		void Add (Aabb<V> rect, T data);
		
		void Remove (Aabb<V> rect, T data);

		IEnumerable<KeyValuePair<Aabb<V>, T>> Overlap (Aabb<V> rect);

		int Count { get; }
	}

	public static class BoundingTree
	{
		public static B FromEnumerable<B, V, T> (IEnumerable<KeyValuePair<Aabb<V>, T>> pairs)
			where B : IBoundingTree<V, T>, new ()
			where V : struct, IVec<V, float>
		{
			var result = new B ();
			foreach (var pair in pairs)
				result.Add (pair.Key, pair.Value);
			return result;
		}
	}
}

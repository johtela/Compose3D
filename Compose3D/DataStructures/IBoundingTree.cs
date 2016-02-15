namespace Compose3D.DataStructures
{
	using System.Collections.Generic;
	using Maths;

	interface IBoundingTree<V, T>
		where V : struct, IVec<V, float>
	{
		void Add (Aabb<V> rect, T data);
		IEnumerable<KeyValuePair<Aabb<V>, T>> Overlap (Aabb<V> rect);
		void Remove (Aabb<V> rect, T data);
	}
}

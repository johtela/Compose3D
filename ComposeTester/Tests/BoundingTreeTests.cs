namespace ComposeTester.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Compose3D;
	using Compose3D.Maths;
	using Compose3D.DataStructures;
	using LinqCheck;

	public class BoundingTreeTests
	{
		static BoundingTreeTests ()
		{
			Arbitrary.Register (new Arbitrary<Aabb<Vec2>> (GenAabb<Vec2> ()));
			Arbitrary.Register (new Arbitrary<Aabb<Vec3>> (GenAabb<Vec3> ()));
		}

		public static Gen<Aabb<V>> GenAabb<V> ()
			where V : struct, IVec<V, float>
		{
			var arb = Arbitrary.Get<V> ();
			return from low in arb.Generate
				   let high = low.Multiply (2f)
				   select new Aabb<V> (low, high);
		}

	}
}

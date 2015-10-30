namespace ComposeTester
{
    using System;
    using System.Linq;
    using Compose3D.Arithmetics;
    using LinqCheck;

	public class QuatTests
    {
		static QuatTests ()
        {
			Arbitrary.Register (ArbitraryQuat<Quat, float> ());
        }

		public static Arbitrary<Q> ArbitraryQuat<Q, T> () 
			where Q : struct, IQuat<Q, T>
            where T : struct, IEquatable<T>
        {
            var arb = Arbitrary.Get<T> (); 
			var quat = default (Q);

			return new Arbitrary<Q> ( 
				from a in arb.Generate.FixedArrayOf (4)
				select quat.FromAxisAngle (a[0], a[1], a[2], a[3]));
        }

		public void CheckMultWithIdentity<Q, T> () 
			where Q : struct, IQuat<Q, T>
			where T : struct, IEquatable<T>
        {
			var prop = 
				from quat in Prop.Choose<Q> ()
				let ident = quat.Identity
				select new { quat, ident };

			prop.Label ("{0}: quat * ident = quat", typeof(Q).Name)
				.Check (p => p.quat.Multiply (p.ident).Equals (p.quat));
			prop.Label ("{0}: | quat | = 1", typeof(Q).Name)
				.Check (p => p.quat.IsNormalized);
        }

		public void CheckMultiplication<Q, T> () 
			where Q : struct, IQuat<Q, T>
			where T : struct, IEquatable<T>
		{
			var prop = 
				from quat1 in Prop.Choose<Q> ()
				from quat2 in Prop.Choose<Q> ()
				let prod = quat1.Multiply (quat2).Normalized
				select new { quat1, quat2, prod };

			prop.Label ("{0}: | quat1 * quat2 | / len = 1", typeof(Q).Name)
				.Check (p => p.prod.IsNormalized);
		}

		public void CheckMatrixConversion<Q, T, V, M>()
			where Q : struct, IQuat<Q, T>
			where T : struct, IEquatable<T>
			where V : struct, IVec<V, T>
			where M : struct, ISquareMat<M, T>
		{
			var prop =
				from quat in Prop.Choose<Q> ()
				from vec in Prop.Choose<V> ()
				let vecLen = vec.Length
				let mat = quat.ToMatrix<M> ()
				let transVec = mat.Multiply (vec)
				let transVecLen = transVec.Length
				select new { quat, vec, vecLen, mat, transVec, transVecLen };

			prop.Label ("{0}: quat = mat => | mat * vec | = | vec |", typeof (Q).Name)
				.Check (p => p.vecLen.Equals (p.transVecLen));
		}

		[Test]
		public void TestMultiplication ()
        {
			CheckMultWithIdentity<Quat, float> ();
			CheckMultiplication<Quat, float> ();
        }

		[Test]
		public void TestMatrixConversion ()
		{
			CheckMatrixConversion<Quat, float, Vec3, Mat3> ();
		}
    }
}

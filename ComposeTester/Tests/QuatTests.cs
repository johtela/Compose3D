namespace ComposeTester
{
    using System;
    using System.Linq;
	using Extensions;
    using Compose3D.Maths;
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

		public void CheckRotatingVec<Q, T, V> ()
			where Q : struct, IQuat<Q, T>
			where T : struct, IEquatable<T>
			where V : struct, IVec<V, T>
		{
			var prop =
				from quat in Prop.Choose<Q> ()
				from vec in Prop.Choose<V> ()
				let vecLen = vec.Length
				let rotVec = quat.RotateVec (vec)
				let rotVecLen = rotVec.Length
				select new { quat, vec, vecLen, rotVec, rotVecLen };

			prop.Label ("{0}: | quat * vec | = | vec |", typeof (Q).Name)
				.Check (p => p.vecLen.ApproxEquals (p.rotVecLen));
		}

		public void CheckMatrixConversion<Q, T, V, M> ()
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
				.Check (p => p.vecLen.ApproxEquals (p.transVecLen));
		}

		public void CheckLerping<Q> (Func<Q, Q, float, Q> lerpFunc)
			where Q : struct, IQuat<Q, float>
		{
			var prop =
				from quat1 in Prop.Choose<Q> ()
				from quat2 in Prop.Choose<Q> ()
				from alpha in Prop.ForAll (Gen.ChooseDouble (0.0, 1.0).ToFloat ())
				let lerp = lerpFunc (quat1, quat2, alpha)
				let len = lerp.Length
				select new { quat1, quat2, alpha, lerp, len };

			prop.Label ("{0}: | lerp (quat1, quat2) | = 1", typeof (Q).Name)
				.Check (p => p.lerp.IsNormalized);
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

		[Test]
		public void TestVecRotation ()
		{
			CheckRotatingVec<Quat, float, Vec3> ();
		}

		[Test]
		public void TestLerping ()
		{
			CheckLerping<Quat> ((q1, q2, a) => q1.Lerp (q2, a));
			CheckLerping<Quat> ((q1, q2, a) => q1.Slerp (q2, a));
		}
	}
}

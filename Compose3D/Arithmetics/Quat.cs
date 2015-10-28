namespace Compose3D.Arithmetics
{
	using System;

	public struct Quat : IQuat<Quat, float>
	{
		private const float LERP_THRESHOLD = 0.99f;

		public Vec3 Vec;
		public float W;
		public static readonly Quat Identity = new Quat (new Vec3 (0f), 1f);

		public Quat (Vec3 vec, float w)
		{
			Vec = vec;
			W = w;
		}

		public Quat (float x, float y, float z, float w)
		{
			Vec.X = x;
			Vec.Y = y;
			Vec.Z = z;
			W = w;
		}

		public Vec4 ToVec4 ()
		{
			return new Vec4 (Vec.X, Vec.Y, Vec.Z, W);
		}

		public static Quat FromVec4 (Vec4 vec)
		{
			return new Quat (vec.X, vec.Y, vec.Z, vec.W);
		}

		public static Quat FromAxisAngle (Vec3 axis, float angle)
		{
			var lensqr = axis.LengthSquared;
			if (angle == 0f || lensqr == 0f)
				return Identity;

			var normaxis = lensqr == 1f ? axis : axis / lensqr.Sqrt ();
			var halfangle = angle / 2;
			return new Quat (normaxis * halfangle.Sin (), halfangle.Cos ());
		}

		public M ToMatrix<M> () where M : struct, ISquareMat<M, float>
		{
			var xx = Vec.X * Vec.X;
			var xy = Vec.X * Vec.Y;
			var xz = Vec.X * Vec.Z;
			var xw = Vec.X * W;
			var yy = Vec.Y * Vec.Y;
			var yz = Vec.Y * Vec.Z;
			var yw = Vec.Y * W;
			var zz = Vec.Z * Vec.Z;
			var zw = Vec.Z * W;

			var res = Mat.Identity<M> ();
			res [0, 0] = 1 - 2 * (yy + zz);
			res [0, 1] = 2 * (xy - zw);
			res [0, 2] = 2 * (xz + yw);
			res [1, 0] = 2 * (xy + zw);
			res [1, 1] = 1 - 2 * (xx + zz);
			res [1, 2] = 2 * (yz - xw);
			res [2, 0] = 2 * (xz - yw);
			res [2, 1] = 2 * (yz + xw);
			res [2, 2] = 1 - 2 * (xx + yy);
			return res;
		}

		public Quat Invert ()
		{
			return new Quat (Vec, -W);
		}

		public Quat Conjugate ()
		{
			return new Quat (-Vec, W);
		}

		public Quat Multiply (Quat other)
		{
			return new Quat (other.W * Vec + W * other.Vec + Vec.Cross (other.Vec),
				W * other.W + Vec.Dot (other.Vec));
		}

		public Quat Lerp (Quat other, float interPos)
		{
			return FromVec4 (ToVec4 ().Mix (other.ToVec4 (), interPos).Normalized);
		}

		public Quat Slerp (Quat other, float interPos)
		{
			var v1 = ToVec4 ();
			var v2 = other.ToVec4 ();
			var dot = v1.Dot (v2);
			if (dot > LERP_THRESHOLD)
				return FromVec4 (v1.Mix (v2, interPos));

			var theta = dot.Acos () * interPos;
			var v3 = (v2 - v1 * dot).Normalized;
			return FromVec4 (v1 * theta.Cos () + v3 * theta.Sin ());
		}

		public float Length
		{
			get { return ToVec4 ().Length; }
		}

		public float LengthSquared
		{
			get { return ToVec4 ().LengthSquared; }
		}

		public Quat Normalized
		{
			get { return FromVec4 (ToVec4 ().Normalized); }
		}

		public static implicit operator Vec4 (Quat quat)
		{
			return quat.ToVec4 ();
		}

		public static implicit operator Quat (Vec4 vec)
		{
			return FromVec4 (vec);
		}

		public static Quat operator - (Quat quat)
		{
			return quat.Invert ();
		}

		public static Quat operator * (Quat left, Quat right)
		{
			return left.Multiply (right);
		}

		public static bool operator == (Quat left, Quat right)
		{
			return left.Equals (right);
		}

		public static bool operator != (Quat left, Quat right)
		{
			return !left.Equals (right);
		}

		public override bool Equals (object obj)
		{
			return obj is Quat && Equals ((Quat)obj);
		}

		public override int GetHashCode ()
		{
			return Vec.GetHashCode () ^ W.GetHashCode ();
		}

		public override string ToString ()
		{
			return string.Format ("[ {0} {1} ]", Vec, W);
		}

		#region IEquatable implementation

		public bool Equals (Quat other)
		{
			return Vec == other.Vec && W == other.W;
		}

		#endregion
	}
}


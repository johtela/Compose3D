namespace ComposeFX.Maths
{
	using System;

	public interface IQuat<Q, T> : IEquatable<Q>
		where Q : struct, IQuat<Q, T>
		where T : struct, IEquatable<T>
	{
		Q Invert ();
		Q Conjugate ();
		Q Multiply (Q other);
		Q FromAxisAngle (T x, T y, T z, T angle);
		V ToVector<V> () where V : struct, IVec<V, T>;
		M ToMatrix<M> () where M : struct, ISquareMat<M, T>;
		V RotateVec<V> (V vec) where V : struct, IVec<V, T>;
		Q Lerp (Q other, T interPos);
		Q Slerp (Q other, T interPos);

		Q Identity { get; }
		T Length { get; }
		T LengthSquared { get; }
		bool IsNormalized { get; }
		Q Normalized { get; }
	}
}
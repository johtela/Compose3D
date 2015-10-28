namespace Compose3D.Arithmetics
{
	using System;

	public interface IQuat<Q, T> : IEquatable<Q>
		where Q : struct, IQuat<Q, T>
		where T : struct, IEquatable<T>
	{
		Q Invert ();
		Q Conjugate ();
		Q Multiply (Q other);

		T Length { get; }
		T LengthSquared { get; }
		Q Normalized { get; }
	}
}
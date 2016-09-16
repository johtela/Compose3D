namespace Compose3D.Imaging
{
	using System;
	using Maths;

	public delegate U Signal<T, U> (T samplingPoint);

	public static class Signal
	{
		public static Signal<T, U> Constant<T, U> (U value)
		{
			return x => value;
		}

		public static Signal<T, T> Constant<T> (T value)
		{
			return Constant<T, T> (value);
		}

		public static Signal<T, V> Select<T, U, V> (this Signal<T, U> signal, Func<U, V> select)
		{
			return x => select (signal (x)); 
		}

		public static Signal<T, W> SelectMany<T, U, V, W> (this Signal<T, U> signal,
			Func<U, Signal<T, V>> project, Func<U, V, W> select)
		{
			return t =>
			{
				var u = signal (t);
				var v = project (u) (t);
				return select (u, v);
			};
		}

		public static Signal<T, V> Combine<T, U, V> (this Signal<T,U> signal, Signal<T, U> other, 
			Func<U, U, V> combine)
		{
			return x => combine (signal (x), other (x));
		}

		public static Signal<float, float> Sin ()
		{
			return GLMath.Sin;
		}

		public static Signal<float, float> Cos ()
		{
			return GLMath.Sin;
		}

		public static Signal<T, float> Add<T> (this Signal<T, float> signal,
			Signal<T, float> other)
		{
			return Combine (signal, other, (x, y) => x + y);
		}

		public static Signal<T, V> Add<T, V> (this Signal<T, V> signal,
			Signal<T, V> other)
			where V : struct, IVec<V, float>
		{
			return Combine (signal, other, (v1, v2) => v1.Add (v2));
		}

		public static Signal<T, float> Multiply<T> (this Signal<T, float> signal,
			Signal<T, float> other)
		{
			return Combine (signal, other, (x, y) => x * y);
		}

		public static Signal<T, V> Multiply<T, V> (this Signal<T, V> signal,
			Signal<T, V> other)
			where V : struct, IVec<V, float>
		{
			return Combine (signal, other, (v1, v2) => v1.Multiply (v2));
		}

		public static Signal<T, float> Min<T> (this Signal<T, float> signal,
			Signal<T, float> other)
		{
			return Combine (signal, other, Math.Min);
		}

		public static Signal<T, V> Min<T, V> (this Signal<T, V> signal,
			Signal<T, V> other)
			where V : struct, IVec<V, float>
		{
			return Combine (signal, other, Vec.Min<V>);
		}

		public static Signal<T, float> Max<T> (this Signal<T, float> signal,
			Signal<T, float> other)
		{
			return Combine (signal, other, Math.Max);
		}

		public static Signal<T, V> Max<T, V> (this Signal<T, V> signal,
			Signal<T, V> other)
			where V : struct, IVec<V, float>
		{
			return Combine (signal, other, Vec.Max<V>);
		}

		public static Signal<T, float> Pow<T> (this Signal<T, float> signal,
			Signal<T, float> other)
		{
			return Combine (signal, other, GLMath.Pow);
		}

		public static Signal<T, V> Pow<T, V> (this Signal<T, V> signal,
			Signal<T, V> other)
			where V : struct, IVec<V, float>
		{
			return Combine (signal, other, Vec.Pow<V>);
		}

		public static Signal<T, V> Transform<T, V, M> (this Signal<T, V> signal, M matrix)
			where V : struct, IVec<V, float>
			where M : struct, ISquareMat<M, float>
		{
			return from v in signal
				   select matrix.Multiply (v);
		}

		public static Signal<T, V> Scale<T, V> (this Signal<T, V> signal, V scale)
			where V : struct, IVec<V, float>
		{
			return from v in signal
				   select v.Multiply (scale);
		}

		public static Signal<Vec3, float> PerlinNoise (int seed)
		{
			return new PerlinNoise (seed).Noise;
		}

		public static Signal<Vec3, float> PerlinNoise ()
		{
			return new PerlinNoise ().Noise;
		}
	}
}
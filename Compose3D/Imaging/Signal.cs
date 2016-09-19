namespace Compose3D.Imaging
{
	using System;
	using Maths;
	using System.Diagnostics;
	using System.Threading.Tasks;

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

		public static Signal<T, V> To<T, U, V> (this Signal<T, U> signal, Signal<U, V> other)
		{
			return x => other (signal (x));
		}

		public static Signal<T, V> Func<T, U, V> (this Func<U, V> func, Func<T, U> mapDomain)
		{
			return x => func (mapDomain (x));
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

		public static Signal<T, uint> NormalRangeToGrayscale<T> (this Signal<T, Vec4> signal)
		{
			var h = 127.5f;
			return signal.Select (vec =>
				(uint)(vec.X * h + h) << 24 |
				(uint)(vec.Y * h + h) << 16 |
				(uint)(vec.Z * h + h) << 8 |
				(uint)(vec.W * h + h));
		}

		public static Signal<T, uint> NormalRangeToGrayscale<T> (this Signal<T, float> signal)
		{
			return signal.Select (x =>
			{
				var c = (uint)(x * 127.5f + 127.5f);
				return c << 24 | c << 16 | c << 8 | 255;
			});
		}

		public static T[] SampleToBuffer<T> (this Signal<Vec2i, T> signal, Vec2i bufferSize)
		{
			var length = bufferSize.Producti ();
			var result = new T[length];
			Parallel.For (0, bufferSize.Y, y =>
			{
				for (int x = 0; x < bufferSize.X; x++)
					result[y * bufferSize.Y + x] = signal (new Vec2i (x, y));
			});
			return result;
		}

		public static Func<Vec2i, Vec3> BitmapToVec3 (Vec2i bitmapSize, float scale)
		{
			return vec => new Vec3 (
				vec.X * scale / bitmapSize.X,
				vec.Y * scale / bitmapSize.Y,
				0f);
		}

		public static Func<Vec2i, float> BitmapXToFloat (Vec2i bitmapSize, float scale)
		{
			return vec => vec.X * scale / bitmapSize.X;
		}

		public static Func<Vec2i, float> BitmapYToFloat (Vec2i bitmapSize, float scale)
		{
			return vec => vec.X * scale / bitmapSize.X;
		}
	}
}
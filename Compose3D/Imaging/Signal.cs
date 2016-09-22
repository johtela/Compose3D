namespace Compose3D.Imaging
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using Extensions;
	using Maths;

	public delegate U Signal<T, U> (T samplePoint);

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

		public static Signal<T, V> Combine<T, U, V> (this Signal<T, U> signal, Signal<T, U> other,
			Func<U, U, V> combine)
		{
			return x => combine (signal (x), other (x));
		}

		public static Signal<T, V> To<T, U, V> (this Signal<T, U> signal, Signal<U, V> other)
		{
			return x => other (signal (x));
		}

		public static Signal<V, U> MapDomain<T, U, V> (this Signal<T, U> signal, Func<V, T> map)
		{
			return x => signal (map (x));
		}

		public static Signal<T, V> Transform<T, V, M> (this Signal<T, V> signal, M matrix)
			where V : struct, IVec<V, float>
			where M : struct, ISquareMat<M, float>
		{
			return from v in signal
				   select matrix.Multiply (v);
		}

		public static Signal<T, float> Scale<T> (this Signal<T, float> signal, float scale)
		{
			return signal.Select (x => x * scale);
		}

		public static Signal<T, V> Scale<T, V> (this Signal<T, V> signal, V scale)
			where V : struct, IVec<V, float>
		{
			return signal.Select (v => v.Multiply (scale));
		}

		public static Signal<T, float> NormalRangeToZeroOne<T> (this Signal<T, float> signal)
		{
			return signal.Select (x => x * 0.5f + 0.5f);
		}

		public static Signal<T, uint> Vec4ToUintColor<T> (this Signal<T, Vec4> signal)
		{
			var h = 255f;
			return signal.Select (vec =>
				(uint)(vec.X.Clamp (0f, 1f) * h) << 24 |
				(uint)(vec.Y.Clamp (0f, 1f) * h) << 16 |
				(uint)(vec.Z.Clamp (0f, 1f) * h) << 8 |
				(uint)(vec.W.Clamp (0f, 1f) * h));
		}

		public static Signal<T, uint> Vec3ToUintColor<T> (this Signal<T, Vec3> signal)
		{
			var h = 255f;
			return signal.Select (vec =>
				(uint)(vec.X.Clamp (0f, 1f) * h) << 24 |
				(uint)(vec.Y.Clamp (0f, 1f) * h) << 16 |
				(uint)(vec.Z.Clamp (0f, 1f) * h) << 8 | 255);
		}

		public static Signal<T, uint> FloatToUintGrayscale<T> (this Signal<T, float> signal)
		{
			return signal.Select (x =>
			{
				var c = (uint)(x.Clamp (0f, 1f) * 255f);
				return c << 24 | c << 16 | c << 8 | 255;
			});
		}

		public static Signal<T, V> Colorize<T, V> (this Signal<T, float> signal, ColorMap<V> colorMap)
			where V : struct, IVec<V, float>
		{
			return signal.Select (x => colorMap[x]);
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

		public static Func<Vec2i, Vec3> BitmapCoordToVec3 (Vec2i bitmapSize, float scale)
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

		public static Signal<Vec2i, float> BitmapYToFloat (Vec2i bitmapSize, float scale)
		{
			return vec => vec.X * scale / bitmapSize.X;
		}

		public static Signal<V, float> SpectralControl<V> (this Signal<V, float> signal, int startBand,
			int endBand, params float[] bandWeights)
			where V : struct, IVec<V, float>
		{
			if (startBand > endBand)
				throw new ArgumentException ("endBand must be greater or equal to startBand");
			if (bandWeights.Length != (endBand - startBand) + 1)
				throw new ArgumentException ("Invalid number of bands");
			var sumWeights = bandWeights.Aggregate (0f, (s, w) => s + w);
			var normWeights = bandWeights.Map (w => w / sumWeights);
			return vec =>
			{
				var result = 0f;
				for (int i = startBand; i <= endBand; i++)
				{
					float factor = 1 << i;
					result += signal (vec.Multiply (factor)) * normWeights[i - startBand];
				}
				return result;
			};
		}
	}
}
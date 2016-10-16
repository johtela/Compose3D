namespace Compose3D.Imaging
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using Extensions;
	using Maths;

	public delegate U Signal<in T, out U> (T samplePoint);

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

		public static Signal<T, W> Combine<T, U, V, W> (this Signal<T, U> signal, Signal<T, V> other,
			Func<U, V, W> combine)
		{
			return x => combine (signal (x), other (x));
		}

		public static Signal<T, S> Combine<T, U, V, W, S> (this Signal<T, U> signal1, Signal<T, V> signal2,
			Signal<T, W> signal3, Func<U, V, W, S> combine)
		{
			return x => combine (signal1 (x), signal2 (x), signal3 (x));
		}

		public static Signal<T, V> To<T, U, V> (this Signal<T, U> signal, Signal<U, V> other)
		{
			return x => other (signal (x));
		}

		public static Signal<V, U> MapInput<T, U, V> (this Signal<T, U> signal, Func<V, T> map)
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

		public static Signal<float, float> Dfdx (this Signal<float, float> signal, float dx)
		{
			return x => (signal (x + dx) - signal (x)) / dx;
		}

		public static Signal<V, float> Dfdx<V> (this Signal<V, float> signal, int dimension, float dx)
			where V : struct, IVec<V, float>
		{
			var delta = default (V).With (dimension, dx);
			return v => (signal (v.Add (delta)) - signal (v)) / dx;
		}

		public static Signal<V, V> Dfdv<V> (this Signal<V, float> signal, float dv)
			where V : struct, IVec<V, float>
		{
			return v =>
			{
				var result = default (V);
				var value = signal (v);
				for (int i = 0; i < result.Dimensions; i++)
					result[i] = signal (v.Add (default (V).With (i, dv))) - value;
				return result.Divide (dv);
			};
		}

		public static Signal<V, float> Warp<V> (this Signal<V, float> signal, Signal<V, float> warp, float dv)
			where V : struct, IVec<V, float>
		{
			var warpDv = warp.Dfdv (dv);
			return v => signal (v.Add (warpDv (v)));
		}

		public static Signal<T, float> Blend<T> (this Signal<T, float> signal, Signal<T, float> other, 
			float blendFactor)
		{
			if (blendFactor < 0f || blendFactor > 1f)
				throw new ArgumentException ("Blend factor must be in range [0, 1]");
			return signal.Combine (other, (x, y) => GLMath.Mix (x, y, blendFactor));
		}

		public static Signal<T, V> Blend<T, V> (this Signal<T, V> signal, Signal<T, V> other,
			float blendFactor)
			where V : struct, IVec<V, float>
		{
			if (blendFactor < 0f || blendFactor > 1f)
				throw new ArgumentException ("Blend factor must be in range [0, 1]");
			return signal.Combine (other, (v1, v2) => v1.Mix (v2, blendFactor));
		}

		public static Signal<T, float> Mask<T> (this Signal<T, float> signal, Signal<T, float> other,
			Signal<T, float> mask)
		{
			return signal.Combine (other, mask, (x, y, m) => GLMath.Mix (x, y, m));
		}

		public static Signal<T, V> Mask<T, V> (this Signal<T, V> signal, Signal<T, V> other,
			Signal<T, float> mask)
			where V : struct, IVec<V, float>
		{
			return signal.Combine (other, mask, (v1, v2, m) => v1.Mix (v2, m));
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

		public static Func<Vec2i, Vec2> BitmapCoordToUnitRange (Vec2i bitmapSize, float scale)
		{
			return vec => new Vec2 (
				vec.X * scale / bitmapSize.X,
				vec.Y * scale / bitmapSize.Y);
		}

		public static Func<Vec2i, float> BitmapXToFloat (Vec2i bitmapSize, float scale)
		{
			return vec => vec.X * scale / bitmapSize.X;
		}

		public static Signal<Vec2i, float> BitmapYToFloat (Vec2i bitmapSize, float scale)
		{
			return vec => vec.X * scale / bitmapSize.X;
		}

		public static Signal<V, float> SpectralControl<V> (this Signal<V, float> signal, int firstBand,
			int lastBand, params float[] bandWeights)
			where V : struct, IVec<V, float>
		{
			if (firstBand < 0)
				throw new ArgumentException ("Bands must be positive");
			if (firstBand > lastBand)
				throw new ArgumentException ("lastBand must be greater or equal to firstBand");
			if (lastBand > 15)
				throw new ArgumentException ("lastBand must be less than 16.");
			if (bandWeights.Length != (lastBand - firstBand) + 1)
				throw new ArgumentException ("Invalid number of bands");
			var sumWeights = bandWeights.Aggregate (0f, (s, w) => s + w);
			var normWeights = bandWeights.Map (w => w / sumWeights);
			return vec =>
			{
				var result = 0f;
				for (int i = firstBand; i <= lastBand; i++)
				{
					float factor = 1 << i;
					result += signal (vec.Multiply (factor)) * normWeights[i - firstBand];
				}
				return result;
			};
		}
	}
}
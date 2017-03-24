namespace Compose3D.Imaging
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Concurrent;
	using System.Linq;
	using System.Threading.Tasks;
	using Extensions;
	using Maths;
	using DataStructures;

	public delegate U Signal<in T, out U> (T samplePoint);

	public enum DistanceKind { Euclidean, Manhattan }
	public enum WorleyNoiseKind { F1, F2, F3, F2_F1 }

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
			return x =>  matrix.Multiply (signal (x));
		}

		public static Signal<T, float> Scale<T> (this Signal<T, float> signal, float scale)
		{
			return from x in signal
			       select x * scale;
		}

		public static Signal<T, V> Scale<T, V> (this Signal<T, V> signal, V scale)
			where V : struct, IVec<V, float>
		{
			return from v in signal
			       select v.Multiply (scale);
		}

		public static Signal<T, float> Offset<T> (this Signal<T, float> signal, float delta)
		{
			return from x in signal
			       select x + delta;
		}

		public static Signal<T, V> Offset<T, V> (this Signal<T, V> signal, V delta)
			where V : struct, IVec<V, float>
		{
			return from v in signal
			       select v.Add (delta);
		}

		public static Signal<T, float> Clamp<T> (this Signal<T, float> signal, float min, float max)
		{
			return from x in signal
			       select x.Clamp (min, max);
		}

		public static Signal<T, float> NormalRangeToZeroOne<T> (this Signal<T, float> signal)
		{
			return from x in signal
			       select x * 0.5f + 0.5f;
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

		public static Signal<V, V> Dfdv<V> (this Signal<V, float> signal, V dv)
			where V : struct, IVec<V, float>
		{
			return v =>
			{
				var result = default (V);
				var value = signal (v);
				for (int i = 0; i < result.Dimensions; i++)
					result[i] = signal (v.Add (default (V).With (i, dv[i]))) - value;
				return result.Divide (dv);
			};
		}

		public static Signal<V, float> Warp<V> (this Signal<V, float> signal, Signal<V, float> warp, V dv)
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
			return signal.Combine (other, (x, y) => FMath.Mix (x, y, blendFactor));
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
			return signal.Combine (other, mask, (x, y, m) => FMath.Mix (x, y, m));
		}

		public static Signal<T, V> Mask<T, V> (this Signal<T, V> signal, Signal<T, V> other,
			Signal<T, float> mask)
			where V : struct, IVec<V, float>
		{
			return signal.Combine (other, mask, (v1, v2, m) => v1.Mix (v2, m));
		}

		public static Signal<Vec2, Vec3> NormalMap (this Signal<Vec2, float> signal, float strength, Vec2 dv)
		{
			var scale = dv * strength;
			return from v in signal.Dfdv (dv)
				   let n = new Vec3 (v * scale, 1f).Normalized
				   select n * 0.5f + new Vec3 (0.5f);
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
            signal.SampleToBuffer (result, bufferSize);
			return result;
		}

        public static void SampleToBuffer<T> (this Signal<Vec2i, T> signal, T[] buffer, Vec2i bufferSize)
        {
            var length = bufferSize.Producti ();
            Parallel.For (0, bufferSize.Y, y =>
            {
                Parallel.For (0, bufferSize.X, x =>
                    buffer[y * bufferSize.Y + x] = signal (new Vec2i (x, y)));
            });
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

		public static Func<Vec2i, float> BitmapYToFloat (Vec2i bitmapSize, float scale)
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

		private static V[] Neighbours<V> (KdTree<V, int> tree, V vec, int num, Func<V, V, float> distance)
			where V : struct, IVec<V, float>
		{
			return tree.NearestNeighbours (vec, num, distance).Keys ().ToArray ();
		}

		public static Signal<V, float> WorleyNoise<V> (WorleyNoiseKind kind, Func<V, V, float> distance,
			IEnumerable<V> controlPoints) where V : struct, IVec<V, float>
		{
			var i = 0;
			var tree = new KdTree<V, int> (controlPoints.Select (cp =>
				new KeyValuePair<V, int> (cp, i++)));
			switch (kind)
			{
				case WorleyNoiseKind.F1:
					return vec => distance (vec, Neighbours (tree, vec, 1, distance)[0]);
				case WorleyNoiseKind.F2:
			 		return vec => distance (vec, Neighbours (tree, vec, 2, distance)[1]);
				case WorleyNoiseKind.F3:
					return vec => distance (vec, Neighbours (tree, vec, 3, distance)[2]);
				default:
					return vec =>
					{
						var neigbours = Neighbours (tree, vec, 2, distance);
						return distance (vec, neigbours[1]) - distance (vec, neigbours[0]);
					};
			}
		}

		public static IEnumerable<V> RandomControlPoints<V> (int seed)
			where V : struct, IVec<V, float>
		{
			var random = new System.Random (seed);
			return EnumerableExt.Generate<V> (() => Vec.Random<V> (random, 0f, 1f));
		}

		public static IEnumerable<Vec2> HaltonControlPoints ()
		{
			var i = 1;
			return EnumerableExt.Generate<Vec2> (() => 
				new Vec2 (FMath.HaltonSequenceItem (2, i), FMath.HaltonSequenceItem (3, i++)));
		}

		public static IEnumerable<V> Jitter<V> (this IEnumerable<V> controlPoints, float maxDelta)
			where V : struct, IVec<V, float>
		{
			return from v in controlPoints
				   select v.Jitter (maxDelta);
		}

		public static IEnumerable<Vec2> ReplicateOnTorus (this IEnumerable<Vec2> controlPoints)
		{
			foreach (var cp in controlPoints)
			{
				var nx = cp.X + (cp.X < 0.5f ? 1f : -1f);
				var ny = cp.Y + (cp.Y < 0.5f ? 1f : -1f);
				yield return cp;
				yield return new Vec2 (nx, cp.Y);
				yield return new Vec2 (cp.X, ny);
				yield return new Vec2 (nx, ny);
			}
		}

		public static Signal<T, V> Cache<T, V> (this Signal<T, V> signal)
		{
			var cache = new ConcurrentDictionary<T, V> ();
			return x =>
			{
				V value;
				if (cache.TryGetValue (x, out value))
					return value;
				value = signal (x);
				cache.TryAdd (x, value);
				return value;
			};
		}
	}
}
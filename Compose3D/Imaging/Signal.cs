namespace Compose3D.Imaging
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Extensions;
	using Maths;

	public class ColorMap<V>
		where V : struct, IVec<V, float>
	{
		public readonly SortedList<float, V> SamplePoints;

		public ColorMap ()
		{
			SamplePoints = new SortedList<float, V> ();
		}

		public ColorMap (IEnumerable<Tuple<float, V>> samplePoints)
			: this ()
		{
			foreach (var sample in samplePoints)
				SamplePoints.Add (sample.Item1, sample.Item2);
		}

		public ColorMap (params Tuple<float, V>[] samplePoints)
			: this ((IEnumerable<Tuple<float, V>>)samplePoints)
		{ }

		public V this[float value]
		{
			get
			{
				if (SamplePoints.Count == 0)
					throw new InvalidOperationException ("No values in the map");
				var keys = SamplePoints.Keys;
				var values = SamplePoints.Values;
				if (value <= keys[0])
					return values[0];
				var last = SamplePoints.Count - 1;
				if (last == 0 || value >= keys[last])
					return values[last];
				var i = keys.FirstIndex (k => k > value);
				var low = keys[i - 1];
				var high = keys[i];
				return values[i - 1].Mix (values[i], (value - low) / (high - low));
			}
		}
	}

	public static class Signal
	{
		public static Func<T, U> Constant<T, U> (U value)
		{
			return x => value;
		}

		public static Func<T, T> Constant<T> (T value)
		{
			return Constant<T, T> (value);
		}

		public static Func<T, V> Select<T, U, V> (this Func<T, U> signal, Func<U, V> select)
		{
			return x => select (signal (x)); 
		}

		public static Func<T, W> SelectMany<T, U, V, W> (this Func<T, U> signal,
			Func<U, Func<T, V>> project, Func<U, V, W> select)
		{
			return t =>
			{
				var u = signal (t);
				var v = project (u) (t);
				return select (u, v);
			};
		}

		public static Func<T, V> Combine<T, U, V> (this Func<T,U> signal, Func<T, U> other, 
			Func<U, U, V> combine)
		{
			return x => combine (signal (x), other (x));
		}

		public static Func<T, V> To<T, U, V> (this Func<T, U> signal, Func<U, V> other)
		{
			return x => other (signal (x));
		}

		public static Func<T, V> Func<T, U, V> (this Func<U, V> func, Func<T, U> mapDomain)
		{
			return x => func (mapDomain (x));
		}

		public static Func<T, V> Transform<T, V, M> (this Func<T, V> signal, M matrix)
			where V : struct, IVec<V, float>
			where M : struct, ISquareMat<M, float>
		{
			return from v in signal
				   select matrix.Multiply (v);
		}

		public static Func<T, V> Scale<T, V> (this Func<T, V> signal, V scale)
			where V : struct, IVec<V, float>
		{
			return from v in signal
				   select v.Multiply (scale);
		}

		public static Func<T, uint> Vec4ToUintColor<T> (this Func<T, Vec4> signal)
		{
			var h = 127.5f;
			return signal.Select (vec =>
				(uint)(vec.X * h + h) << 24 |
				(uint)(vec.Y * h + h) << 16 |
				(uint)(vec.Z * h + h) << 8 |
				(uint)(vec.W * h + h));
		}

		public static Func<T, uint> Vec3ToUintColor<T> (this Func<T, Vec3> signal)
		{
			var h = 127.5f;
			return signal.Select (vec =>
				(uint)(vec.X * h + h) << 24 |
				(uint)(vec.Y * h + h) << 16 |
				(uint)(vec.Z * h + h) << 8 | 255);
		}

		public static Func<T, uint> FloatToUintGrayscale<T> (this Func<T, float> signal)
		{
			return signal.Select (x =>
			{
				var c = (uint)(x * 127.5f + 127.5f);
				return c << 24 | c << 16 | c << 8 | 255;
			});
		}

		public static Func<T, V> Colorize<T, V> (this Func<T, float> signal, ColorMap<V> colorMap)
			where V : struct, IVec<V, float>
		{
			return signal.Select (x => colorMap[x]);
		}

		public static T[] SampleToBuffer<T> (this Func<Vec2i, T> signal, Vec2i bufferSize)
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
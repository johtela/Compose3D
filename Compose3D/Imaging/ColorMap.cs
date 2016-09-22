namespace Compose3D.Imaging
{
	using System;
	using System.Collections.Generic;
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
}

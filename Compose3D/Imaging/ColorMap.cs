namespace Compose3D.Imaging
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using Extensions;
	using Maths;

	public class ColorMap<V> : IEnumerable<KeyValuePair<float, V>>
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

		public void Add (float key, V value)
		{
			SamplePoints.Add (key, value);
		}

		public IEnumerator<KeyValuePair<float, V>> GetEnumerator ()
		{
			return SamplePoints.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public IEnumerable<KeyValuePair<float, V>> NormalizedSamplePoints (float keyMin, float keyMax)
		{
			var domain = keyMax - keyMin;
			return SamplePoints.Select (p => new KeyValuePair<float, V> (p.Key - keyMin / domain, p.Value));
		}

		public float MinKey
		{
			get { return SamplePoints.Keys[0]; }
		}

		public float MaxKey
		{
			get { return SamplePoints.Keys[SamplePoints.Count - 1]; }
		}

		public int Count
		{
			get { return SamplePoints.Count; }
		}

		public V this[float key]
		{
			get
			{
				if (SamplePoints.Count == 0)
					throw new InvalidOperationException ("No values in the map");
				var keys = SamplePoints.Keys;
				var values = SamplePoints.Values;
				if (key <= keys[0])
					return values[0];
				var last = SamplePoints.Count - 1;
				if (last == 0 || key >= keys[last])
					return values[last];
				var i = keys.FirstIndex (k => k > key);
				var low = keys[i - 1];
				var high = keys[i];
				return values[i - 1].Mix (values[i], (key - low) / (high - low));
			}
			set
			{
				var i = SamplePoints.Keys.IndexOf (key);
				if (i >= 0)
					SamplePoints.Values[i] = value;
				else
					SamplePoints.Add (key, value);
			}
		}

		public V this[int index]
		{
			get { return SamplePoints.Values[index]; }
			set { SamplePoints.Values[index] = value; }
		}
	}
}

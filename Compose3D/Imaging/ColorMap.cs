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
		private SortedList<float, V> _samplePoints;

		public ColorMap ()
		{
			_samplePoints = new SortedList<float, V> ();
		}

		public ColorMap (IEnumerable<Tuple<float, V>> samplePoints)
			: this ()
		{
			if (samplePoints.IsEmpty ())
				throw new ArgumentException ("Need to provide at least one sample point to color map");
			foreach (var sample in samplePoints)
				_samplePoints.Add (sample.Item1, sample.Item2);
		}

		public void Add (float key, V value)
		{
			_samplePoints.Add (key, value);
		}

		public void Remove (float key)
		{
			if (_samplePoints.Count == 1)
				throw new InvalidOperationException ("Color map has to contain at least one sample point.");
			_samplePoints.Remove (key);
		}

		public bool MoveKey (float key, float newKey)
		{
			var i = _samplePoints.IndexOfKey (key);
			if ((i < 0) ||
				(key == newKey) ||
				(i > 0 && _samplePoints.Keys[i - 1] >= newKey) ||
				(i < _samplePoints.Count - 1 && _samplePoints.Keys [i + 1] <= newKey))
				return false;
			var value = _samplePoints.Values[i];
			_samplePoints.RemoveAt (i);
			_samplePoints.Add (newKey, value);
			return true;
		}

		public IEnumerator<KeyValuePair<float, V>> GetEnumerator ()
		{
			return _samplePoints.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public IEnumerable<KeyValuePair<float, V>> NormalizedSamplePoints (float keyMin, float keyMax)
		{
			var domain = keyMax - keyMin;
			return _samplePoints.Select (p => new KeyValuePair<float, V> ((p.Key - keyMin) / domain, p.Value));
		}

		public float MinKey
		{
			get { return _samplePoints.Keys[0]; }
		}

		public float MaxKey
		{
			get { return _samplePoints.Keys[_samplePoints.Count - 1]; }
		}

		public int Count
		{
			get { return _samplePoints.Count; }
		}

		public V this[float key]
		{
			get
			{
				if (_samplePoints.Count == 0)
					throw new InvalidOperationException ("No values in the map");
				var keys = _samplePoints.Keys;
				var values = _samplePoints.Values;
				if (key <= keys[0])
					return values[0];
				var last = _samplePoints.Count - 1;
				if (last == 0 || key >= keys[last])
					return values[last];
				var i = keys.FirstIndex (k => k > key);
				var low = keys[i - 1];
				var high = keys[i];
				return values[i - 1].Mix (values[i], (key - low) / (high - low));
			}
			set
			{
				var i = _samplePoints.Keys.IndexOf (key);
				if (i >= 0)
					_samplePoints.Values[i] = value;
				else
					_samplePoints.Add (key, value);
			}
		}

		public V this[int index]
		{
			get { return _samplePoints.Values[index]; }
			set { _samplePoints.Values[index] = value; }
		}
	}
}

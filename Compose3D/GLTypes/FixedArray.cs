namespace Compose3D.GLTypes
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public abstract class FixedArray<T> : IEnumerable<T>
	{
		private T[] _array;

		protected FixedArray (T[] array)
		{
			_array = array;
		}

		public int Length
		{
			get { return _array.Length; }
		}

		public T this[int index]
		{
			get { return _array[index]; }
			set { _array[index] = value; }
		}

		public IEnumerator<T> GetEnumerator ()
		{
			return ((IEnumerable<T>)_array).GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((IEnumerable<T>)_array).GetEnumerator ();
		}
	}

	public class FixedArray2<T> : FixedArray<T>
	{
		public FixedArray2 () : base (new T[2])	{ }
	}

	public class FixedArray3<T> : FixedArray<T>
	{
		public FixedArray3 () : base (new T[3]) { }
	}

	public class FixedArray4<T> : FixedArray<T>
	{
		public FixedArray4 () : base (new T[4]) { }
	}

	public class FixedArray5<T> : FixedArray<T>
	{
		public FixedArray5 () : base (new T[5]) { }
	}

	public class FixedArray6<T> : FixedArray<T>
	{
		public FixedArray6 () : base (new T[6]) { }
	}
}

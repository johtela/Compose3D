namespace Extensions
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	public class PrintableEnumerable<T> : IEnumerable<T>
	{
		IEnumerable<T> _enumerable;
		
		public PrintableEnumerable (IEnumerable<T> enumerable)
		{
			_enumerable = enumerable;
		}

		public IEnumerator<T> GetEnumerator ()
		{
			return _enumerable.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return _enumerable.GetEnumerator ();
		}
		
		public override string ToString ()
		{
			return this.ToArray ().ToString ("[ ", " ]", ", ");
		}
	}
}


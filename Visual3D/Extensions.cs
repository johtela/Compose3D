namespace Visual3D
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class Extensions
	{
		public static T Next<T> (this IEnumerator<T> enumerator)
		{
			enumerator.MoveNext ();
			return enumerator.Current;
		}
	}
}


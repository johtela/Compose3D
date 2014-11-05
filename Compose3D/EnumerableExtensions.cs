namespace Compose3D
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class EnumerableExtensions
	{
		public static T Next<T> (this IEnumerator<T> enumerator)
		{
			enumerator.MoveNext ();
			return enumerator.Current;
		}

		public static IEnumerable<T> Repeat<T> (this IEnumerable<T> enumerable)
		{
			while (true)
			{
				var enumerator = enumerable.GetEnumerator ();
				while (enumerator.MoveNext ())
					yield return enumerator.Current;
			}
		}
	}
}


namespace Compose3D
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
    using OpenTK;

	public static class Extensions
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

        public static Vector3 Transform (this Vector3 vec, Matrix3 mat)
        {
            return new Vector3 (
                vec.X * mat.Row0.X + vec.Y * mat.Row1.X + vec.Z * mat.Row2.X, 
                vec.X * mat.Row0.Y + vec.Y * mat.Row1.Y + vec.Z * mat.Row2.Y, 
                vec.X * mat.Row0.Z + vec.Y * mat.Row1.Z + vec.Z * mat.Row2.Z);
        }
	}
}


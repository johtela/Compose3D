namespace Compose3D.Imaging
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Maths;
	using Extensions;

	internal static class CodeGen
	{
		internal static string ToCode (object obj)
		{
			if (obj is string)
				return "\"" + obj as string + "\"";
			else if (obj is float)
				return ((float)obj).ToString (CultureInfo.InvariantCulture) + "f";
			else if (obj is Enum)
				return obj.GetType ().Name + "." + obj.ToString ();
			else if (obj is bool)
				return obj.ToString ().ToLower ();
			else if (obj is Vec2)
				return VecToCode<Vec2, float> ((Vec2)obj);
			else if (obj is Vec3)
				return VecToCode<Vec3, float> ((Vec3)obj);
			else if (obj is Vec4)
				return VecToCode<Vec4, float> ((Vec4)obj);
			else if (obj is Vec2i)
				return VecToCode<Vec2i, int> ((Vec2i)obj);
			else if (obj is Vec3i)
				return VecToCode<Vec3i, int> ((Vec3i)obj);
			else if (obj is Vec4i)
				return VecToCode<Vec4i, int> ((Vec4i)obj);
			else if (obj is ColorMap<Vec3>)
				return ColorMapToCode (obj as ColorMap<Vec3>);
			else if (obj is ColorMap<Vec4>)
				return ColorMapToCode (obj as ColorMap<Vec4>);
			else if (obj is IEnumerable)
				return (obj as IEnumerable).Cast<object> ().Select (ToCode).SeparateWith (", ");
			else if (obj is AnySignalEditor)
				return (obj as AnySignalEditor).Name;
			else
				return obj.ToString ();
		}

		private static string ColorMapToCode<V> (ColorMap<V> colorMap)
			where V : struct, IVec<V, float>
		{
			return string.Format ("new ColorMap<{0}> {{ {1} }}", typeof (V).Name,
				colorMap.Select (kv => string.Format ("{{ {0}, {1} }}", 
					ToCode (kv.Key), ToCode (kv.Value)))
				.SeparateWith (", "));
		}

		private static string VecToCode<V, T> (this V vec)
			where V : struct, IVec<V, T>
			where T: struct, IEquatable<T>
		{
			return string.Format ("new {0} ({1})", typeof (V).Name,
				vec.ToArray<V, T> ().Select (i => ToCode (i)).SeparateWith (", "));
		}
	}
}

namespace Compose3D.Maths
{
    using System;
	using System.Collections.Generic;
	using System.Linq;
	using GLTypes;

	public interface IVec<V, T> : IEquatable<V>
		where V : struct, IVec<V, T>
		where T : struct, IEquatable<T>
	{
		V Invert ();
		V Add (V other);
		V Subtract (V other);
		V Multiply (T scalar);
		V Multiply (V scale);
		V Divide (T scalar);
		T Dot (V other);

		int Dimensions { get; }
		T this[int index] { get; set; }
		T Length { get; }
		T LengthSquared { get; }
		V Normalized { get; }
	}
	
    public static class Vec
    {
        public static V FromArray<V, T> (params T[] items)
            where V : struct, IVec<V, T>
            where T : struct, IEquatable<T>
        {
            var res = default (V);
            for (int i = 0; i < Math.Min (res.Dimensions, items.Length); i++)
                res[i] = items[i];
            return res;
        }

        public static T[] ToArray<V, T> (this V vec)
            where V : struct, IVec<V, T>
            where T : struct, IEquatable<T>
        {
            var res = new T[vec.Dimensions];
            for (int i = 0; i < vec.Dimensions; i++)
                res[i] = vec[i];
            return res;
        }

		public static bool ApproxEquals<V> (V vec, V other, float epsilon)
            where V : struct, IVec<V, float>
        {
            for (int i = 0; i < vec.Dimensions; i++)
				if (!vec[i].ApproxEquals (other[i], epsilon)) 
					return false;
            return true;
        }

		public static bool ApproxEquals<V> (V vec, V other)
			where V : struct, IVec<V, float>
		{
			return ApproxEquals (vec, other, 0.000001f);
		}

        public static V With<V, T> (this V vec, int i, T value)
            where V : struct, IVec<V, T>
            where T : struct, IEquatable<T>
        {
            var res = vec;
            res[i] = value;
            return res;
        }
		
		public static float Sum<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			var res = vec [0];
			for (int i = 1; i < vec.Dimensions; i++)
				res += vec [i];
			return res;
		}
		
		public static IEnumerable<V> Interpolate<V> (this V from, V to, int steps)
			where V : struct, IVec<V, float>
		{
			var step = 1f / steps;
			var f = 0f;
			for (int i = 0; i < steps; i++, f += step)
				yield return from.Mix (to, f);
		}

		public static V Map<V, T> (this V vec, Func<T, T> map)
			where V : struct, IVec<V, T>
			where T : struct, IEquatable<T>
		{
			var res = default (V);
			for (int i = 0; i < vec.Dimensions; i++)
				res[i] = map (vec[i]);
			return res;
		}

		public static V Map2<V, T> (this V vec, V other, Func<T, T, T> map)
			where V : struct, IVec<V, T>
			where T : struct, IEquatable<T>
		{
			var res = default (V);
			for (int i = 0; i < vec.Dimensions; i++)
				res[i] = map (vec[i], other[i]);
			return res;
		}

		public static V Map3<V, T> (this V vec1, V vec2, V vec3, Func<T, T, T, T> map)
			where V : struct, IVec<V, T>
			where T : struct, IEquatable<T>
		{
			var res = default (V);
			for (int i = 0; i < vec1.Dimensions; i++)
				res[i] = map (vec1[i], vec2[i], vec3[i]);
			return res;
		}

		public static Vec3 CalculateNormal (this Vec3 position, Vec3 adjacentPos1, Vec3 adjacentPos2)
		{
			if (position == adjacentPos1 || position == adjacentPos2 || adjacentPos1 == adjacentPos2)
				throw new ArgumentException (
					"The positions need to be unique in oreder to calculate the normal correctly.");
			return (adjacentPos1 - position).Cross (adjacentPos2 - position).Normalized;
		}

		[GLFunction ("cross ({0})")]
		public static Vec3 Cross (this Vec3 v1, Vec3 v2)
		{
			return new Vec3 (
				v1.Y * v2.Z - v1.Z * v2.Y,
				v1.Z * v2.X - v1.X * v2.Z,
				v1.X * v2.Y - v1.Y * v2.X);			
		}

		[GLFunction ("radians ({0})")]
		public static V Radians<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (GLMath.Radians);
		}

		[GLFunction ("degrees ({0})")]
		public static V Degrees<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (GLMath.Degrees);
		}

		[GLFunction ("abs ({0})")]
		public static V Abs<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (Math.Abs);
		}

		[GLFunction ("floor ({0})")]
		public static V Floor<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (GLMath.Floor);
		}

		[GLFunction ("ceil ({0})")]
		public static V Ceiling<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (GLMath.Ceiling);
		}

		[GLFunction ("min ({0})")]
		public static V Min<V> (this V vec, V other)
			where V : struct, IVec<V, float>
		{
			return vec.Map2<V, float> (other, Math.Min);
		}

		[GLFunction ("min ({0})")]
		public static V Mini<V> (this V vec, V other)
			where V : struct, IVec<V, int>
		{
			return vec.Map2<V, int> (other, Math.Min);
		}

		[GLFunction ("max ({0})")]
		public static V Max<V> (this V vec, V other)
			where V : struct, IVec<V, float>
		{
			return vec.Map2<V, float> (other, Math.Max);
		}

		[GLFunction ("max ({0})")]
		public static V Maxi<V> (this V vec, V other)
			where V : struct, IVec<V, int>
		{
			return vec.Map2<V, int> (other, Math.Max);
		}

		[GLFunction ("clamp ({0})")]
		public static V Clamp<V> (this V vec, float min, float max)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (a => GLMath.Clamp (a, min, max));
		}

		[GLFunction ("clamp ({0})")]
		public static V Clamp<V> (this V vec, int min, int max)
			where V : struct, IVec<V, int>
		{
			return vec.Map<V, int> (a => GLMath.Clamp (a, min, max));
		}

		[GLFunction ("clamp ({0})")]
		public static V Clamp<V> (this V vec, V min, V max)
			where V : struct, IVec<V, float>
		{
			return vec.Map3<V, float> (min, max, GLMath.Clamp);
		}

		[GLFunction ("clamp ({0})")]
		public static V Clampi<V> (this V vec, V min, V max)
			where V : struct, IVec<V, int>
		{
			return vec.Map3<V, int> (min, max, GLMath.Clamp);
		}

		[GLFunction ("mix ({0})")]
		public static V Mix<V> (this V vec, V other, V interPos)
			where V : struct, IVec<V, float>
		{
			return vec.Map3<V, float> (other, interPos, GLMath.Mix);
		}

		[GLFunction ("mix ({0})")]
		public static V Mix<V> (this V vec, V other, float interPos)
			where V : struct, IVec<V, float>
		{
			return vec.Map2<V, float> (other, (x, y) => GLMath.Mix (x, y, interPos));
		}

		[GLFunction ("step ({0})")]
		public static V Step<V> (float edge, V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (a => GLMath.Step (edge, a));
		}

		[GLFunction ("step ({0})")]
		public static V Step<V> (V edge, V vec)
			where V : struct, IVec<V, float>
		{
			return edge.Map2<V, float> (vec, GLMath.Step);
		}

		[GLFunction ("smoothstep ({0})")]
		public static V Step<V> (float edgeLower, float edgeUpper, V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (a => GLMath.SmoothStep (edgeLower, edgeUpper, a));
		}

		[GLFunction ("smoothstep ({0})")]
		public static V Step<V> (V edgeLower, V edgeUpper, V vec)
			where V : struct, IVec<V, float>
		{
			return edgeLower.Map3<V, float> (edgeUpper, vec, GLMath.SmoothStep);
		}

		[GLFunction ("pow ({0})")]
		public static V Pow<V> (this V vec, V exp)
			where V : struct, IVec<V, float>
		{
			return vec.Map2<V, float> (exp, GLMath.Pow);
		}

		[GLFunction ("exp ({0})")]
		public static V Exp<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (GLMath.Exp);
		}

		[GLFunction ("log ({0})")]
		public static V Log<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (GLMath.Log);
		}

		[GLFunction ("sqrt ({0})")]
		public static V Sqrt<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (GLMath.Sqrt);
		}

		[GLFunction ("inversesqrt ({0})")]
		public static V InverseSqrt<V> (this V vec)
			where V : struct, IVec<V, float>
		{
			return vec.Map<V, float> (GLMath.InverseSqrt);
		}

		[GLFunction ("reflect ({0})")]
		public static V Reflect<V, T> (this V vec, V along)
			where V : struct, IVec<V, float>
			where T : struct, IEquatable<T>
		{
			return vec.Subtract (along.Multiply (2 * vec.Dot (along)));
		}

		[GLFunction ("sin ({0})")]
		public static V Sin<V> (this V angles)
			where V : struct, IVec<V, float>
		{
			return angles.Map<V, float> (GLMath.Sin);
		}

		[GLFunction ("cos ({0})")]
		public static V Cos<V> (this V angles)
			where V : struct, IVec<V, float>
		{
			return angles.Map<V, float> (GLMath.Cos);
		}

		[GLFunction ("tan ({0})")]
		public static V Tan<V> (this V angles)
			where V : struct, IVec<V, float>
		{
			return angles.Map<V, float> (GLMath.Tan);
		}

		[GLFunction ("asin ({0})")]
		public static V Asin<V> (this V x)
			where V : struct, IVec<V, float>
		{
			return x.Map<V, float> (GLMath.Asin);
		}

		[GLFunction ("acos ({0})")]
		public static V Acos<V> (this V x)
			where V : struct, IVec<V, float>
		{
			return x.Map<V, float> (GLMath.Acos);
		}

		[GLFunction ("atan ({0})")]
		public static V Atan<V> (this V y_over_x)
			where V : struct, IVec<V, float>
		{
			return y_over_x.Map<V, float> (GLMath.Atan);
		}

		[GLFunction ("atan ({0})")]
		public static V Atan2<V> (this V y, V x)
			where V : struct, IVec<V, float>
		{
			return y.Map2<V, float> (x, GLMath.Atan2);
		}
	}
}

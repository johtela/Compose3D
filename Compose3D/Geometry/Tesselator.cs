namespace Compose3D.Geometry
{
	using Arithmetics;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;

	public static class Tesselator<V> where V : struct, IVertex 
	{
		private class TessVertex : IEnumerable<TessVertex>
		{
			public TessVertex Next;
			public TessVertex Previous;

			public int Index;
			public float Angle;
			public bool IsEar;

			public bool IsReflex
			{
				get { return Angle >= MathHelper.Pi; }
			}

			public static TessVertex FromEnumerable (IEnumerable<TessVertex> e)
			{
				var first = e.First ();
				var prev = first;

				foreach (var tv in e.Skip (1))
				{
					prev.Next = tv;
					tv.Previous = prev;
					prev = tv;
				}
				prev.Next = first;
				first.Previous = prev;
				return first;
			}

			public void Delete ()
			{
				Previous.Next = Next;
				Next.Previous = Previous;
			}

			public IEnumerator<TessVertex> GetEnumerator ()
			{
				var curr = this;
				do
				{
					yield return curr;
					curr = curr.Next;
				} 
				while (curr != this);
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}
		}

		public static int[] TesselatePolygon (V[] vertices)
		{
			var count = vertices.Length;
			var result = new int[(count - 2) * 3];
			var resInd = 0;
			var tessVerts = TessVertex.FromEnumerable (
				from i in Enumerable.Range (0, count)
				select new TessVertex () { Index = i });

			foreach (var tv in tessVerts)
				UpdateVertexAngle (tv, vertices);

			foreach (var tv in tessVerts)
				UpdateIsEar (tv, vertices);

			while (count > 3)
			{
				var curr = FindMinimumEar (tessVerts);
				var prev = curr.Previous;
				var next = curr.Next;
				result[resInd++] = prev.Index;
				result[resInd++] = curr.Index;
				result[resInd++] = next.Index;
				curr.Delete ();
				UpdateVertexAngle (prev, vertices);
				UpdateVertexAngle (next, vertices);
				UpdateIsEar (prev, vertices);
				UpdateIsEar (next, vertices);
				tessVerts = next;
				count--;
			}
			foreach (var tv in tessVerts)
				result[resInd++] = tv.Index;

			return result;
		}

		private static void UpdateVertexAngle (TessVertex tessVert, V[] vertices)
		{
			tessVert.Angle = AngleBetweenEdges (
				tessVert.Previous.Index, tessVert.Index, tessVert.Next.Index, vertices);
		}

		private static void UpdateIsEar (TessVertex current, V[] vertices)
		{
			if (current.IsReflex)
				current.IsEar = false;
			else
			{
				var prev = current.Previous;
				var next = current.Next;
				var p0 = vertices[prev.Index].Position;
				var p1 = vertices[current.Index].Position;
				var p2 = vertices[next.Index].Position;

				current.IsEar = current.Where (v => v != current && v != prev && v != next && v.IsReflex)
					.All (cv => !PointInTriangle (vertices[cv.Index].Position, p0, p1, p2));
			}
		}

		private static TessVertex FindMinimumEar (TessVertex first)
		{
			var minAngle = MathHelper.TwoPi;
			var res = first;
			foreach (var vert in first)
				if (vert.IsEar && vert.Angle < minAngle)
				{
					minAngle = vert.Angle;
					res = vert;
				}
			return res;
		}
	
		private static float AngleBetweenEdges (int prev, int current, int next, V[] vertices)
		{
			var vec1 = vertices[prev].Position - vertices[current].Position;
			var vec2 = vertices[next].Position - vertices[current].Position;
			var result = GLMath.Atan2 (vec2.Y, vec2.X) - GLMath.Atan2 (vec1.Y, vec1.X);
			return result < 0 ? MathHelper.TwoPi + result : result;
		}

		private static bool PointInTriangle (Vec3 p, Vec3 p0, Vec3 p1, Vec3 p2)
		{
			var s = (p0.Y * p2.X - p0.X * p2.Y + (p2.Y - p0.Y) * p.X + (p0.X - p2.X) * p.Y);
			var t = (p0.X * p1.Y - p0.Y * p1.X + (p0.Y - p1.Y) * p.X + (p1.X - p0.X) * p.Y);
			if (s <= 0 || t <= 0)
				return false;
			var A = (-p1.Y * p2.X + p0.Y * (-p1.X + p2.X) + p0.X * (p1.Y - p2.Y) + p1.X * p2.Y);
			return (s + t) < A;
		}
	}
}

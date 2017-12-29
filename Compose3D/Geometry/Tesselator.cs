namespace Compose3D.Geometry
{
	using Compose3D.Maths;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using Extensions;

	public static class Tesselator<V> where V : struct, IVertex<Vec3> 
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

            public static TessVertex CircularList (int count)
            {
                var first = new TessVertex { Index = 0 };
				var prev = first;

				for (var i = 1; i < count; i ++)
				{
                    var tv = new TessVertex { Index = i };
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
			if (count < 3)
				throw new ArgumentException ("Tesselator needs at least 3 vertices");
			var result = new int[(count - 2) * 3];
			var resInd = 0;
            var tessVerts = TessVertex.CircularList (count);

			foreach (var tv in tessVerts)
				UpdateVertexAngle (tv, vertices);

			foreach (var tv in tessVerts)
				UpdateIsEar (tv, vertices);

			while (count > 3)
			{
				var curr = tessVerts.Where (v => v.IsEar).MinItems (v => v.Angle).First ();
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
				var p0 = vertices[prev.Index].position;
				var p1 = vertices[current.Index].position;
				var p2 = vertices[next.Index].position;

				current.IsEar = current.Where (v => v != current && v != prev && v != next && v.IsReflex)
					.All (cv => !PointInTriangle (vertices[cv.Index].position, p0, p1, p2));
			}
		}

		private static float AngleBetweenEdges (int prev, int current, int next, V[] vertices)
		{
			var vec1 = vertices[prev].position - vertices[current].position;
			var vec2 = vertices[next].position - vertices[current].position;
			var result = FMath.Atan2 (vec2.Y, vec2.X) - FMath.Atan2 (vec1.Y, vec1.X);
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

namespace Compose3D.Geometry
{
	using Arithmetics;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;

	public static class Tesselator<V> where V : struct, IVertex 
	{
		private class TessVertex
		{
			public int Index;
			public float Angle;
			public bool IsEar;

			public bool IsConvex
			{
				get { return Angle < MathHelper.Pi; }
			}

			public bool IsReflex
			{
				get { return !IsConvex; }
			}
		}

		public static int[] TesselatePolygon (V[] vertices)
		{
			var result = new int[(vertices.Length - 2) * 3];
			var resInd = 0;
			var tessVerts =
				(from i in Enumerable.Range (0, vertices.Length)
				 select new TessVertex () { Index = i }).ToList ();

			for (int i = 0; i < tessVerts.Count; i++)
				tessVerts[i].Angle = VertexAngle (i, tessVerts, vertices);

			for (int i = 0; i < tessVerts.Count; i++)
				tessVerts[i].IsEar = IsEar (i, tessVerts, vertices);

			while (tessVerts.Count > 3)
			{
				var curr = FindMinimumEar (tessVerts);
				var prev = PrevIndex (curr, tessVerts);
				var next = NextIndex (curr, tessVerts);
				result[resInd++] = tessVerts[prev].Index;
				result[resInd++] = tessVerts[curr].Index;
				result[resInd++] = tessVerts[next].Index;
				tessVerts.RemoveAt (curr);
				if (curr == 0) prev--;
				if (next != 0) next--;
				tessVerts[prev].Angle = VertexAngle (prev, tessVerts, vertices);
				tessVerts[next].Angle = VertexAngle (next, tessVerts, vertices);
				tessVerts[prev].IsEar = IsEar (prev, tessVerts, vertices);
				tessVerts[next].IsEar = IsEar (next, tessVerts, vertices);
			}
			for (int i = 0; i < 3; i++)
				result[resInd++] = tessVerts[i].Index;
			return result;
		}

		private static int PrevIndex (int index, List<TessVertex> tessVerts)
		{
			var res = index - 1;
			return res < 0 ? tessVerts.Count + res : res;
		}

		private static int NextIndex (int index, List<TessVertex> tessVerts)
		{
			return (index + 1) % tessVerts.Count;
		}

		private static TessVertex PrevVertex (int index, List<TessVertex> tessVerts)
		{
			return tessVerts[PrevIndex (index, tessVerts)];
		}

		private static TessVertex NextVertex (int index, List<TessVertex> tessVerts)
		{
			return tessVerts[NextIndex (index, tessVerts)];
		}

		private static float VertexAngle (int index, List<TessVertex> tessVerts, V[] vertices)
		{
			return AngleBetweenEdges (
				PrevVertex (index, tessVerts).Index, tessVerts[index].Index, NextVertex (index, tessVerts).Index,
				vertices);
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

		private static bool IsEar (int index, List<TessVertex> tessVerts, V[] vertices)
		{
			var current = tessVerts[index];
			if (current.IsReflex)
				return false;

			var prev = PrevVertex (index, tessVerts);
			var next = NextVertex (index, tessVerts);
			var p0 = vertices[prev.Index].Position;
			var p1 = vertices[current.Index].Position;
			var p2 = vertices[next.Index].Position;

			return tessVerts.Where (v => v != current && v != prev && v != next && v.IsReflex)
				.All (cv => !PointInTriangle (vertices[cv.Index].Position, p0, p1, p2));
		}

		private static int FindMinimumEar (List<TessVertex> tessVerts)
		{
			var minAngle = MathHelper.TwoPi;
			var res = -1;
			for (int i = 0; i < tessVerts.Count; i++)
			{
				var vert = tessVerts[i];
				if (vert.IsEar && vert.Angle < minAngle)
				{
					minAngle = vert.Angle;
					res = i;
				}
			}
			return res;
		}
	}
}

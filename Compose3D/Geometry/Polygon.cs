namespace Compose3D.Geometry
{
	using Arithmetics;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;

	public class Polygon<V> : Primitive<V> where V : struct, IVertex
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

		private int[] _indices;

		public Polygon (V[] vertices, int[] indices) : base (vertices)
		{
			_indices = indices;
		}

		public static Polygon<V> FromVertices (params V[] vertices)
		{
			var tess = new LibTessDotNet.Tess ();
			tess.AddContour (vertices.Map (v =>
			{
				var cv = new LibTessDotNet.ContourVertex ();
				cv.Position.X = v.Position.X;
				cv.Position.Y = v.Position.Y;
				cv.Position.Z = v.Position.Z;
				return cv;
			}),
			LibTessDotNet.ContourOrientation.Original);
			tess.Tessellate (LibTessDotNet.WindingRule.Positive, LibTessDotNet.ElementType.Polygons, 3);
			return new Polygon<V> (vertices, tess.Elements);
		}

		public static Polygon<V> FromVec2s (params Vec2[] vectors)
		{
			return FromVertices (vectors.Map (vec => VertexHelpers.New<V> (new Vec3 (vec, 0f), new Vec3 (0f, 0f, 1f))));
		}

		public int[] Tesselate ()
		{
			var vertices =
				(from i in Enumerable.Range (0, _vertices.Length)
				 select new TessVertex () { Index = i }).ToList ();
			PolulateVertexAngles (vertices);

			for (int i = 0; i < vertices.Count; i++)
				vertices[i].IsEar = IsEar (i, vertices);


		}

		private TessVertex PrevVertex (int index, List<TessVertex> vertices)
		{
			return vertices [Math.Abs ((index - 1) % vertices.Count)];
		}

		private TessVertex NextVertex (int index, List<TessVertex> vertices)
		{
			return vertices [(index + 1) % vertices.Count];
		}

		private void PolulateVertexAngles (List<TessVertex> vertices)
		{
			var len = vertices.Count;
			for (int i = 0; i < len; i++)
				vertices[i].Angle = AngleBetweenEdges (
					PrevVertex (i, vertices).Index, vertices[i].Index, NextVertex (i, vertices).Index);
		}

		private float AngleBetweenEdges (int prev, int current, int next)
		{
			var vec1 = _vertices[prev].Position - _vertices[current].Position;
			var vec2 = _vertices[next].Position - _vertices[current].Position;
			var result = GLMath.Atan2 (vec2.Y, vec2.X) - GLMath.Atan2 (vec1.Y, vec1.X);
			return result < 0 ? MathHelper.TwoPi + result : result;
        }

		private bool PointInTriangle (Vec3 p, Vec3 p0, Vec3 p1, Vec3 p2)
		{
			var s = (p0.Y * p2.X - p0.X * p2.Y + (p2.Y - p0.Y) * p.X + (p0.X - p2.X) * p.Y);
			var t = (p0.X * p1.Y - p0.Y * p1.X + (p0.Y - p1.Y) * p.X + (p1.X - p0.X) * p.Y);
			if (s <= 0 || t <= 0)
				return false;
			var A = (-p1.Y * p2.X + p0.Y * (-p1.X + p2.X) + p0.X * (p1.Y - p2.Y) + p1.X * p2.Y);
			return (s + t) < A;
		}

		private bool IsEar (int index, List<TessVertex> vertices)
		{
			var current = vertices[index];
            if (current.IsReflex)
				return false;

			var p0 = _vertices [PrevVertex (index, vertices).Index].Position;
            var p1 = _vertices [current.Index].Position;
			var p2 = _vertices [NextVertex (index, vertices).Index].Position;

			return vertices.Where (v => v != current && v.IsReflex).All (cv => 
				!PointInTriangle (_vertices [cv.Index].Position, p0, p1, p2));
		}

		protected override IEnumerable<int> GenerateIndices ()
		{
			return _indices;
		}
	}
}


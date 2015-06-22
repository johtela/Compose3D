﻿namespace Compose3D.Geometry
{
    using Arithmetics;
	using System;
    using System.Collections.Generic;

	internal class Quadrilateral<V> : Geometry<V> where V : struct, IVertex
	{
		private Func<Geometry<V>, V[]> _generateVertices;
		private V[] _vertices;
		private int[] _indices;

		private Quadrilateral (Func<Geometry<V>, V[]> generateVertices)
		{
			_generateVertices = generateVertices;
			_indices = CalculateIndices (0);
		}

		public static int[] CalculateIndices (int first)
		{
			return new int[] { first, first + 1, first + 2, first + 2, first + 3, first };
		}

		public static Quadrilateral<V> Rectangle (float width, float height)
		{
			return Trapezoid (width, height, 0f, 0f);
		}

		public static Quadrilateral<V> Parallelogram (float width, float height, float topOffset)
		{
			return Trapezoid (width, height, topOffset, topOffset);
		}
			
		public static Quadrilateral<V> Trapezoid (float width, float height, 
			float topLeftOffset, float topRightOffset)
		{
			var right = width / 2f + topRightOffset;
			var top = height / 2f;
			var left = -right + topLeftOffset;
			var bottom = -top;
			var normal = new Vec3 (0f, 0f, 1f);
			return new Quadrilateral<V> (q =>
			{
				var colors = q.Material.Colors.GetEnumerator ();
				return new V[] 
				{
					Vertex (new Vec3 (right, top, 0f), colors.Next (), normal),
					Vertex (new Vec3 (right, bottom, 0f), colors.Next (), normal),
					Vertex (new Vec3 (left, bottom, 0f), colors.Next (), normal),
					Vertex (new Vec3 (left, top, 0f), colors.Next (), normal)
				};
			});
		}

		public override int VertexCount
		{
			get { return 4; }
		}

		public override IEnumerable<V> Vertices
		{
			get 
			{ 
				if (_vertices == null)
					_vertices = _generateVertices (this);
				return _vertices;
			}
		}

		public override IEnumerable<int> Indices
		{
			get { return _indices; }
		}
	}
}
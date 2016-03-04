namespace Compose3D.Geometry
{
	using System;
	using Maths;
	using DataStructures;

	public class TerrainPatch<V> where V : struct, IVertex
	{
		public readonly Vec2i Start;
		public readonly Vec2i Size;
		public readonly V[] Vertices;
		public readonly int[] Indices;

		private float _minHeight;
		private float _maxHeight;
		private Aabb<Vec3> _boundingBox;
		
		public TerrainPatch (Vec2i start, Vec2i size)
		{
			Start = start;
			Size = size;
			_minHeight = int.MaxValue;
			_maxHeight = int.MinValue;
			Vertices = new V[Size.X * Size.Y];
			Indices = new int[((Size.X * 2) - 1) * (Size.Y - 1) + 1];
			GenerateVertexPositions ();
			GenerateVertexNormals ();
			GenerateIndices ();
		}

		private int Index (int x, int z)
		{
			return z * Size.X + x;
		}

		private float Height (int x, int z)
		{
			return Noise.Noise2D (new Vec2 (Start.X + x, Start.Y + z), 0.049999f, 20f, 3, 10f);
		}
		
		private void GenerateVertexPositions ()
		{
			for (int z = 0; z < Size.Y; z++)
				for (int x = 0; x < Size.X; x++)
				{
					var height = Height (x, z);
					if (height < _minHeight)
						_minHeight = height;
					if (height > _maxHeight)
						_maxHeight = height;
					Vertices[Index (x, z)].Position = new Vec3 (Start.X + x, height, Start.Y + z);
				}
		}

		private void GenerateVertexNormals ()
		{
			for (int z = 0; z < Size.Y; z++)
				for (int x = 0; x < Size.X; x++)
				{
					var w  = Height (x - 1, z);
					var e = Height (x + 1, z);
					var n  = Height (x, z - 1);
					var s = Height (x, z + 1);
					Vertices[Index (x, z)].Normal = new Vec3 (w - e, 2f, n - s).Normalized;
				}
		}

		private void GenerateIndices ()
		{
			var i = 0;
			for (int z = 0; z < Size.Y - 1; z++)
			{
				if ((z & 1) == 1)
					for (int x = 0; x < Size.X; x++)
					{
						Indices[i++] = Index (x, z);
						if (x < Size.X - 1)
							Indices[i++] = Index (x, z + 1);
					}
				else
					for (int x = Size.X - 1; x >= 0; x--)
					{
						Indices[i++] = Index (x, z);
						if (x > 0)
							Indices[i++] = Index (x, z + 1);
					}
			}
			var lastZ = Size.Y - 1;
			Indices[i++] = Index ((lastZ & 1) == 1 ? 0 : Size.X - 1, lastZ);
		}

		public Aabb<Vec3> BoundingBox
		{
			get
			{
				if (_boundingBox == null)
				{
					var start = new Vec3 (Start.X, _minHeight, Start.Y);
					var endi = Start + Size;
					var end = new Vec3 (endi.X, _maxHeight, endi.Y);
					_boundingBox = new Aabb<Vec3> (start, end);
				}
				return _boundingBox;
			}
		}
	}
}
namespace Compose3D.Geometry
{
	using System;
	using Maths;
	using DataStructures;

	public class TerrainPatch<V> where V : struct, IVertex
	{
		public readonly Vec2i Start;
		public readonly Vec2i Size;

		private V[] _vertices;
		private int[] _indices;
		private float _amplitude;
		private Aabb<Vec3> _boundingBox;

		public TerrainPatch (Vec2i start, Vec2i size, float amplitude)
		{
			if (amplitude < 0f)
				throw new ArgumentException ("Amplitude must be positive", "amplitude");
			Start = start;
			Size = size;
			_amplitude = amplitude;
		}

		private int Index (int x, int z)
		{
			return z * Size.X + x;
		}

		private float Height (int x, int z)
		{
			return Noise.Noise2D (new Vec2 (Start.X + x, Start.Y + z), 0.0499999f, _amplitude, 3, _amplitude / 2f);
		}

		private void GenerateVertexPositions ()
		{
			_vertices = new V[Size.X * Size.Y];
			for (int z = 0; z < Size.Y; z++)
				for (int x = 0; x < Size.X; x++)
				{
					var height = Height (x, z);
					_vertices[Index (x, z)].Position = new Vec3 (Start.X + x, height, Start.Y + z);
				}
		}

		private void GenerateVertexNormals ()
		{
			for (int z = 0; z < Size.Y; z++)
				for (int x = 0; x < Size.X; x++)
				{
					var w = Height (x - 1, z);
					var e = Height (x + 1, z);
					var n = Height (x, z - 1);
					var s = Height (x, z + 1);
					_vertices[Index (x, z)].Normal = new Vec3 (w - e, 2f, n - s).Normalized;
				}
		}

		private void GenerateIndices ()
		{
			_indices = new int[((Size.X * 2) - 1) * (Size.Y - 1) + 1];
			var i = 0;
			for (int z = 0; z < Size.Y - 1; z++)
			{
				if ((z & 1) == 1)
					for (int x = 0; x < Size.X; x++)
					{
						_indices[i++] = Index (x, z);
						if (x < Size.X - 1)
							_indices[i++] = Index (x, z + 1);
					}
				else
					for (int x = Size.X - 1; x >= 0; x--)
					{
						_indices[i++] = Index (x, z);
						if (x > 0)
							_indices[i++] = Index (x, z + 1);
					}
			}
			var lastZ = Size.Y - 1;
			_indices[i++] = Index ((lastZ & 1) == 1 ? 0 : Size.X - 1, lastZ);
		}

		public V[] Vertices
		{
			get
			{
				if (_vertices == null)
				{
					GenerateVertexPositions ();
					GenerateVertexNormals ();
				}
				return _vertices;
			}
		}

		public int[] Indices
		{
			get
			{
				if (_indices == null)
					GenerateIndices ();
				return _indices;
			}
		}

		public Aabb<Vec3> BoundingBox
		{
			get
			{
				if (_boundingBox == null)
				{
					var start = new Vec3 (Start.X, -_amplitude / 2f, Start.Y);
					var endi = Start + Size;
					var end = new Vec3 (endi.X, _amplitude / 2f, endi.Y);
					_boundingBox = new Aabb<Vec3> (start, end);
				}
				return _boundingBox;
			}
		}
	}
}
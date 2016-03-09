namespace Compose3D.Geometry
{
	using System;
	using System.Threading.Tasks;
	using Maths;
	using DataStructures;
	using Extensions;

	public class TerrainPatch<V> where V : struct, IVertex
	{
		public readonly Vec2i Start;
		public readonly Vec2i Size;

		private V[] _vertices;
		private int[][] _indices;
		private float _amplitude;
		private float _frequency;
		private int _octaves;
		private float _attenuation;
		private Aabb<Vec3> _boundingBox;
		private bool _genStarged;

		public TerrainPatch (Vec2i start, Vec2i size, float amplitude, float frequency, int octaves,
			float attenuation)
		{
			if (amplitude < 0f)
				throw new ArgumentException ("Amplitude must be positive", "amplitude");
			if (start.X < 0 || start.Y < 0)
				throw new ArgumentException ("The start must be positive");
			if (size.X.NumberOfBitsSet () != 1 && size.Y.NumberOfBitsSet () != 1)
				throw new ArgumentException ("The size components must be power of two");
			Start = start;
			Size = size;
			_amplitude = amplitude;
			_frequency = frequency;
			_octaves = octaves;
			_attenuation = attenuation;
		}

		private int Index (int x, int z)
		{
			return z * Size.X + x;
		}

		private float Height (int x, int z)
		{
			return Noise.Noise2D (new Vec2 (Start.X + x, Start.Y + z), _frequency, _amplitude, 
				_octaves, _attenuation);
		}

		private V[] GenerateVertexPositions ()
		{
			var result = new V[Size.X * Size.Y];
			for (int z = 0; z < Size.Y; z++)
			{
				for (int x = 0; x < Size.X; x++)
				{
					var height = Height (x, z);
					result[Index (x, z)].Position = new Vec3 (Start.X + x, height, Start.Y + z);
				}
			}
			return result;
		}

		private void GenerateVertexNormals (V[] vertices)
		{
			for (int z = 0; z < Size.Y; z++)
			{
				for (int x = 0; x < Size.X; x++)
				{
					var w = Height (x - 1, z);
					var e = Height (x + 1, z);
					var n = Height (x, z - 1);
					var s = Height (x, z + 1);
					vertices[Index (x, z)].Normal = new Vec3 (w - e, 2f, n - s).Normalized;
				}
			}
		}

		private int[] GenerateIndices (int lod)
		{
			var result = new int[(((Size.X >> lod) * 2) - 1) * ((Size.Y >> lod) - 1) + 1];
			var step = 1 << lod;
			var i = 0;
			for (int z = 0; z < Size.Y - step; z += step)
			{
				if ((z & step) != 0)
					for (int x = 0; x < Size.X; x += step)
					{
						result[i++] = Index (x, z);
						if (x < Size.X - step)
							result[i++] = Index (x, z + step);
					}
				else
					for (int x = Size.X - step; x >= 0; x -= step)
					{
						result[i++] = Index (x, z);
						if (x > 0)
							result[i++] = Index (x, z + step);
					}
			}
			result[i++] = Index (0, Size.Y - step);
			return result;
		}

		private void Generate ()
		{
			var verts = GenerateVertexPositions ();
			GenerateVertexNormals (verts);
			_vertices = verts;
			var inds = new int[3][];
			for (int i = 0; i < 3; i++)
				inds[i] = GenerateIndices (i);
			_indices = inds;
		}

		public V[] Vertices
		{
			get
			{
				if (_vertices == null && !_genStarged)
				{
					_genStarged = true;
					Task.Run (() => Generate ());
				}
				return _vertices;
			}
		}

		public int[][] Indices
		{
			get
			{
				if (_indices == null && !_genStarged)
				{
					_genStarged = true;
					Task.Run (() => Generate ());
				}
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
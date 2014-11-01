namespace Visual3D
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;

	internal class Transform<V> : Geometry<V> where V : struct, IVertex
	{
		private Matrix4 _matrix;
		private Geometry<V> _geometry;

		public Transform (Geometry<V> geometry, Matrix4 matrix)
		{
			_geometry = geometry;
			_matrix = matrix;
		}

		public override int VertexCount
		{
			get { return _geometry.VertexCount; }
		}

		public override IEnumerable<V> Vertices
		{
			get
			{
				return _geometry.Vertices.Select (v => Vertex (Vector4.Transform (v.Position, _matrix), v.Color));
			}
		}

		public override IEnumerable<int> Indices
		{
			get { return _geometry.Indices; }
		}

		public override IMaterial Material
		{
			get { return base.Material; }
			set
			{
				if (!_geometry.HasMaterial)
					_geometry.Material = value;
				base.Material = value;
			}
		}
	}
}

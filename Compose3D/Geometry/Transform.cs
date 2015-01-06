namespace Compose3D.Geometry
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;

	internal class Transform<V> : Geometry<V> where V : struct, IVertex
	{
        private Geometry<V> _geometry;
        private Matrix4 _matrix;
        private Matrix3 _normalMatrix;

		public Transform (Geometry<V> geometry, Matrix4 matrix)
		{
			_geometry = geometry;
			_matrix = matrix;
            _normalMatrix = new Matrix3 (matrix);
            //_normalMatrix.Invert ();
            //_normalMatrix.Transpose ();
		}

		public override int VertexCount
		{
			get { return _geometry.VertexCount; }
		}

		public override IEnumerable<V> Vertices
		{
			get
			{
				return _geometry.Vertices.Select (v => 
                    Vertex (Vector3.Transform (v.Position, _matrix), 
                        v.Color,
                        v.Normal.Transform (_normalMatrix).Normalized ()));
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

namespace Compose3D.Geometry
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;
    using Arithmetics;

	internal class Transform<V> : Geometry<V> where V : struct, IVertex
	{
        private Geometry<V> _geometry;
        private Mat4 _matrix;
        private Mat3 _normalMatrix;

		public Transform (Geometry<V> geometry, Mat4 matrix)
		{
			_geometry = geometry;
			_matrix = matrix;
            _normalMatrix = _matrix.ConvertTo<Mat4, Mat3, float> ();
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
                    Vertex (new Vec3 (_matrix * new Vec4 (v.Position, 1f)), 
                        v.Color,
                        (_normalMatrix * v.Normal).Normalized));
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

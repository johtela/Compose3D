namespace Compose3D.Geometry
{
    using Arithmetics;
    using System.Collections.Generic;
    using System.Linq;

	internal class Transform<V> : Geometry<V> where V : struct, IVertex
	{
        private Geometry<V> _geometry;
        private Mat4 _matrix;
        private Mat3 _normalMatrix;

		public Transform (Geometry<V> geometry, Mat4 matrix)
		{
			_geometry = geometry;
			_matrix = matrix;
            _normalMatrix = new Mat3(_matrix);
            //_normalMatrix.Invert ();
            //_normalMatrix.Transpose ();
		}

		protected override IEnumerable<V> GenerateVertices ()
		{
			return _geometry.Vertices.Select (v => 
                    Vertex (new Vec3 (_matrix * new Vec4 (v.Position, 1f)), 
				v.Color,
				(_normalMatrix * v.Normal).Normalized));
		}

		protected override IEnumerable<int> GenerateIndices ()
		{
			return _geometry.Indices;
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

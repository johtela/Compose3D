namespace Compose3D.Geometry
{
    using Compose3D.Maths;
    using System.Collections.Generic;
    using System.Linq;

	internal class Transform<V> : Wrapper<V> where V : struct, IVertex
	{
        internal Mat4 _matrix;

		public Transform (Geometry<V> geometry, Mat4 matrix)
			: base (geometry)
		{
			_matrix = matrix;
		}

		protected override IEnumerable<V> GenerateVertices ()
		{
            var normalMatrix = new Mat3 (_matrix).Inverse.Transposed;
            return _geometry.Vertices.Select (v =>
            {
                v.position = new Vec3 (_matrix * new Vec4 (v.position, 1f));
                v.normal = (normalMatrix * v.normal).Normalized;
                return v;
            });
        }
	}
}

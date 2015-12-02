namespace Compose3D.Geometry
{
	using Maths;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public delegate V Manipulator<V> (V input) where V : struct, IVertex;

	internal class VertexManipulator<V> : Wrapper<V>
		where V : struct, IVertex
	{
		private Manipulator<V> _manipulator;
		
		public VertexManipulator(Geometry<V> geometry, Manipulator<V> manipulator)
			: base (geometry)
		{
			_manipulator = manipulator;
		}
		
		protected override IEnumerable<V> GenerateVertices ()
		{
			return from v in _geometry.Vertices 
				   select _manipulator (v);
		}	
	}

	public static class Manipulators
	{
		public static Geometry<V> ManipulateVertices<V> (this Geometry<V> geometry, Manipulator<V> manipulator) 
			where V : struct, IVertex
		{
			return new VertexManipulator<V> (geometry, manipulator);
		}

		public static Manipulator<V> Where<V> (this Manipulator<V> manipulator, Func<V, bool> predicate)
			where V : struct, IVertex
		{
			return vertex => predicate (vertex) ? manipulator (vertex) : vertex;
		}

		public static Manipulator<V> Compose<V> (this Manipulator<V> manipulator, Manipulator<V> other)
			where V : struct, IVertex
		{
			return vertex => other (manipulator (vertex));
		}

		public static Manipulator<V> Transform<V> (Mat4 matrix)
			where V : struct, IVertex
		{
			var nmat = new Mat3 (matrix).Inverse.Transposed;
			return v => v.With (matrix.Transform (v.Position), nmat * v.Normal);
		}

		public static Manipulator<V> Translate<V> (float offsetX, float offsetY, float offsetZ) 
			where V : struct, IVertex
		{
			return Transform<V> (Mat.Translation<Mat4> (offsetX, offsetY, offsetZ));
		}

		public static Manipulator<V> Scale<V> (float factorX, float factorY, float factorZ) 
			where V : struct, IVertex
		{
			return Transform<V> (Mat.Scaling<Mat4> (factorX, factorY, factorZ));
		}

		public static Manipulator<V> Rotate<V> (float angleX, float angleY, float angleZ) 
			where V : struct, IVertex
		{
			var matrix = Mat.RotationZ<Mat4> (angleZ);
			if (angleX != 0.0f) matrix *= Mat.RotationX<Mat4> (angleX);
			if (angleY != 0.0f) matrix *= Mat.RotationY<Mat4> (angleY);
			return Transform<V> (matrix);
		}
	}
}
namespace Compose3D.Geometry
{
	using Maths;
	using System;
	using System.Collections.Generic;

	internal class VertexManipulator<V> : Wrapper<V>
		where V : struct, IVertex
	{
		private Func<V, bool> _selector;
		private Func<V, V> _manipulator;
		
		public VertexManipulator(Geometry<V> geometry, Func<V, bool> selector, Func<V, V> manipulator)
			: base (geometry)
		{
			_selector = selector;
			_manipulator = manipulator;
		}
		
		protected override IEnumerable<V> GenerateVertices ()
		{
			foreach (var v in _geometry.Vertices)
				yield return _selector (v) ? _manipulator (v) : v;
		}	
	}
	
	public static class Manipulators
	{
		public static Geometry<V> ManipulateVertices<V> (this Geometry<V> geometry,
			Func<V, bool> selector, Func<V, V> manipulator) where V : struct, IVertex
		{
			return new VertexManipulator<V> (geometry, selector, manipulator);
		}
		
		public static Func<V, V> Transform<V> (Mat4 matrix)
			where V : struct, IVertex
		{
			var nmat = new Mat3 (matrix).Inverse.Transposed;
			return v => v.With (matrix.Transform (v.Position), nmat * v.Normal);
		}

		public static Func<V, V> Translate<V> (float offsetX, float offsetY, float offsetZ) 
			where V : struct, IVertex
		{
			return Transform<V> (Mat.Translation<Mat4> (offsetX, offsetY, offsetZ));
		}

		public static Func<V, V> Scale<V> (float factorX, float factorY, float factorZ) 
			where V : struct, IVertex
		{
			return Transform<V> (Mat.Scaling<Mat4> (factorX, factorY, factorZ));
		}

		public static Func<V, V> Rotate<V> (float angleX, float angleY, float angleZ) 
			where V : struct, IVertex
		{
			var matrix = Mat.RotationZ<Mat4> (angleZ);
			if (angleX != 0.0f) matrix *= Mat.RotationX<Mat4> (angleX);
			if (angleY != 0.0f) matrix *= Mat.RotationY<Mat4> (angleY);
			return Transform<V> (matrix);
		}
	}
}


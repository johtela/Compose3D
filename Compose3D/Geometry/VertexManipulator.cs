﻿namespace Compose3D.Geometry
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
			return v => v.With (matrix.Transform (v.position), nmat * v.normal);
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

		public static Manipulator<V> RotateX<V> (float angle) 
			where V : struct, IVertex
		{
			return Transform<V> (Mat.RotationX<Mat4> (angle));
		}

		public static Manipulator<V> RotateY<V> (float angle)
			where V : struct, IVertex
		{
			return Transform<V> (Mat.RotationY<Mat4> (angle));
		}
	
		public static Manipulator<V> RotateZ<V> (float angle)
			where V : struct, IVertex
		{
			return Transform<V> (Mat.RotationZ<Mat4> (angle));
		}
	}
}
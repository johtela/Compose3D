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
		public static Geometry<V> ManipulateVertices<V> (this Geometry<V> geometry, Manipulator<V> manipulator, 
			bool recalculateNormals) where V : struct, IVertex
		{
			var result = new VertexManipulator<V> (geometry, manipulator);
			if (recalculateNormals)
				result.RecalculateNormals ();
			return result;
		}

		public static void RecalculateNormals<V> (this Geometry<V> geometry)
			where V : struct, IVertex
		{
			var verts = geometry.Vertices;
			for (int i = 0; i < geometry.Indices.Length; i += 3)
			{
				var i1 = geometry.Indices [i];
				var i2 = geometry.Indices [i + 1];
				var i3 = geometry.Indices [i + 2];
				var normal = verts [i2].position.CalculateNormal (verts [i1].position, verts [i3].position);
				if (!normal.IsNaN () && verts [i1].normal.Dot (normal) > 0f)
				{
					verts [i1].normal = normal;
					verts [i2].normal = normal;
					verts [i3].normal = normal;
				}
			}
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

		public static Manipulator<V> JitterPosition<V> (float range)
			where V : struct, IVertex
		{
			var half = range / 2f;
			var rangeMin = range - half;
			var rangeMax = range + half;
			return v => v.With (
				v.position + Vec.Random<Vec3> (new Random (v.position.GetHashCode ()), rangeMin, rangeMax), 
				v.normal);
		}

		public static Manipulator<V> JitterColor<V> (float range)
			where V : struct, IVertex, IDiffuseColor<Vec3>
		{
			var half = range / 2f;
			var rangeMin = range - half;
			var rangeMax = range + half;
			return v =>
			{ 
				v.diffuse += Vec.Random<Vec3> (new Random (v.position.GetHashCode ()), rangeMin, rangeMax);
				return v;
			};
		}
	}
}
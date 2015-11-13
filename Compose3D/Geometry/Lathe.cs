namespace Compose3D.Geometry
{
	using Compose3D.Maths;
	using OpenTK;
	using System;
	using System.Collections.Generic;

	public class Lathe<V> : Primitive<V> where V : struct, IVertex
	{
		public Lathe (V[] vertices) : base (vertices)
		{ }

		public static Lathe<V> Turn<P> (Path<P, Vec3> path, Axis turnAxis, Vec3 offset,
			float stepAngle, float startAngle, float endAngle)
			where P : struct, IPositional<Vec3>
		{
			if (startAngle > endAngle)
				throw new ArgumentException ("Start angle must be bigger than end angle");
			var pathLen = path.Vertices.Length;
			if (pathLen < 2)
				throw new ArgumentException ("Path must contain at least two positions");
			if (startAngle == endAngle)
				endAngle += MathHelper.TwoPi;
			var radialCount = (int)Math.Ceiling ((endAngle - startAngle) / stepAngle) * 2;
			var vertices = new V [radialCount * (pathLen - 1) * 2];
			var angle = startAngle;
			var vertInd = 0;
			for (var i = 0; i < radialCount; i += 2)
			{
				var pos1 = Positions (path, turnAxis, angle, offset);
				angle = Math.Min (angle + stepAngle, endAngle);
				var pos2 = Positions (path, turnAxis, angle, offset);
				for (int j = 0; j < pathLen - 1; j++)
				{
					var normal = pos1[j].CalculateNormal (pos2[j + 1], pos1[j + 1]);
					vertices[vertInd++] = VertexHelpers.New<V> (pos1[j], normal);
					vertices[vertInd++] = VertexHelpers.New<V> (pos1[j + 1], normal);
					vertices[vertInd++] = VertexHelpers.New<V> (pos2[j + 1], normal);
					vertices[vertInd++] = VertexHelpers.New<V> (pos2[j], normal);
				}
			}
			return new Lathe<V> (vertices);

		}

		private static Vec3[] Positions<P> (Path<P, Vec3> path, Axis axis, float angle, Vec3 offset)
			where P : struct, IPositional<Vec3>
		{
			return path.Vertices.Map (v => Rotate (axis, angle, v.Position, offset));
		}

		private static Vec3 Rotate (Axis axis, float angle, Vec3 vertex, Vec3 offset)
		{
			var result = vertex + offset;
			switch (axis)
			{
				case Axis.X:
					return Mat.RotationX<Mat3> (angle) * result;
				case Axis.Y:
					return Mat.RotationY<Mat3> (angle) * result;
				default:
					return Mat.RotationZ<Mat3> (angle) * result;
			}
		}

		protected override IEnumerable<int> GenerateIndices ()
		{
			for (int i = 0; i < Vertices.Length; i += 4)
			{
				yield return i;
				yield return i + 1;
				yield return i + 2;
				yield return i + 2;
				yield return i + 3;
				yield return i ;
			}
		}
	}
}


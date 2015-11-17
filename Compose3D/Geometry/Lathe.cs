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
				throw new ArgumentException (
					"Start angle must be less or equal than the end angle.If you want to turn " +
					"the path a full circle, set the startAngle and endAngle to the same value.", 
					"startAngle");
			var pathLen = path.Nodes.Length;
			var fullCircle = startAngle == endAngle;
			if (fullCircle)
				endAngle += MathHelper.TwoPi;
			if (stepAngle > (endAngle - startAngle))
				throw new ArgumentException (
					"StepAngle must to be less or equal than the sweep angle (endAngle - stargAngle)", 
					"stepAngle");

			var radialCount = (int)Math.Ceiling ((endAngle - startAngle) / stepAngle) * 2;
			var vertices = new V [radialCount * (pathLen - 1) * 2];
			var angle = startAngle;
			var vertInd = 0;
			var path1 = Positions (path, turnAxis, angle, offset);
			var firstPos = path1;
			for (var i = 0; i < radialCount; i += 2)
			{
				angle = Math.Min (angle + stepAngle, endAngle);
				var path2 = fullCircle && i == radialCount - 2 ? 
					firstPos : 
					Positions (path, turnAxis, angle, offset);
				for (int j = 0; j < pathLen - 1; j++)
				{
					var normal = path1[j].CalculateNormal (path2[j + 1], path1[j + 1]);
					vertices[vertInd++] = VertexHelpers.New<V> (path1[j], normal);
					vertices[vertInd++] = VertexHelpers.New<V> (path1[j + 1], normal);
					vertices[vertInd++] = VertexHelpers.New<V> (path2[j + 1], normal);
					vertices[vertInd++] = VertexHelpers.New<V> (path2[j], normal);
				}
				path1 = path2;
			}
			return new Lathe<V> (vertices);

		}

		private static Vec3[] Positions<P> (Path<P, Vec3> path, Axis axis, float angle, Vec3 offset)
			where P : struct, IPositional<Vec3>
		{
			return path.Nodes.Map (v => Rotate (axis, angle, v.Position, offset));
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


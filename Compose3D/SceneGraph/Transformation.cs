namespace Compose3D.SceneGraph
{
	using Compose3D.Maths;
	using Geometry;
	using System;

	public class Transformation : SceneNode
	{
		public Transformation (Mat4 matrix, Mat3 normalMatrix)
		{
			Matrix = matrix;
			NormalMatrix = normalMatrix;
		}

		public Mat4 Matrix { get; set; }
		public Mat3 NormalMatrix { get; set; }

		public override void Traverse<T> (Action<T, Mat4, Mat3> action, Mat4 transform, Mat3 normalTransform)
		{
			base.Traverse<T> (action, transform * Matrix, normalTransform * NormalMatrix);
		}
	}
}

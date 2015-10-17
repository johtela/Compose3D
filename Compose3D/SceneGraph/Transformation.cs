namespace Compose3D.SceneGraph
{
	using Arithmetics;
	using Geometry;
	using System;

	public class Transformation : SceneNode
	{
		public Transformation (Mat4 matrix)
		{
			Matrix = matrix;
		}

		public Mat4 Matrix { get; set; }

		public override void Traverse<T> (Action<T, Mat4> action, Mat4 transform)
		{
			base.Traverse<T> (action, transform * Matrix);
		}
	}
}

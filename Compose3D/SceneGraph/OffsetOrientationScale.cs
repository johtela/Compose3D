namespace Compose3D.SceneGraph
{
	using Compose3D.Maths;
	using Geometry;
	using System;

	public class OffsetOrientationScale : SceneNode
	{
		public OffsetOrientationScale (Vec3 offset, Vec3 orientation, Vec3 scale)
		{
			Offset = offset;
			Orientation = orientation;
			Scale = scale;
		}

		public Vec3 Offset { get; set; }
		public Vec3 Orientation { get; set; }
		public Vec3 Scale { get; set; }

		public override void Traverse<T> (Action<T, Mat4, Mat3> action, Mat4 transform, Mat3 normalTransform)
		{
			base.Traverse<T> (action, 
				transform *
				Mat.Translation<Mat4> (Offset.X, Offset.Y, Offset.Z) *
				Mat.Scaling<Mat4> (Scale.X, Scale.Y, Scale.Z) *
				Mat.RotationZ<Mat4> (Orientation.Z) *
				Mat.RotationY<Mat4> (Orientation.Y) *
				Mat.RotationX<Mat4> (Orientation.X),
				(normalTransform *
				Mat.RotationZ<Mat3> (Orientation.Z) *
				Mat.RotationY<Mat3> (Orientation.Y) *
				Mat.RotationX<Mat3> (Orientation.X)).Inverse.Transposed
			);
		}
	}
}
namespace Compose3D.SceneGraph
{
	using DataStructures;
	using Maths;
	using Extensions;

	public class Camera : SceneNode
	{
		public Vec3 Position;
		public Vec3 Target;
		public Vec3 UpDirection;
		public ViewingFrustum Frustum;
		
		public Camera (SceneGraph graph, Vec3 position, Vec3 target, Vec3 upDirection, ViewingFrustum frustum, 
			float aspectRatio) : base (graph)
		{
			Position = position;
			Target = target;
			UpDirection = upDirection;
			Frustum = frustum;
		}

		public override Mat4 Transform
		{
			get { return CameraToWorld; }
		}

		public override Aabb<Vec3> BoundingBox
		{
			get
			{
				return Aabb<Vec3>.FromPositions (Frustum.Corners.Map (pos => 
					Transform.Transform (pos)));
			}
		}

		public Mat4 WorldToCamera 
		{
			get { return base.Transform * Mat.LookAt (Position, Target, UpDirection); }
		}

		public Mat4 CameraToWorld
		{
			get { return WorldToCamera.Inverse; }
		}		
	}
}
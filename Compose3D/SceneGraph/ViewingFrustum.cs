namespace Compose3D.SceneGraph
{
	using System;
	using Maths;
	using Geometry;

	public enum FrustumKind	{ Perspective, Orthographic }

	public class ViewingFrustum
	{
		public readonly FrustumKind Kind;
		public readonly float Left;
		public readonly float Right;
		public readonly float Bottom;
		public readonly float Top;
		public readonly float Near;
		public readonly float Far;

		private Mat4? _cameraToScreen;
		private Vec3 [] _corners;

		public ViewingFrustum (FrustumKind kind, float left, float right, float bottom, float top, float near, float far)
		{
			Kind = kind;
			Left = left;
			Right = right;
			Bottom = bottom;
			Top = top;
			Near = near;
			Far = far;
		}
		
		public ViewingFrustum (FrustumKind kind, float width, float height, float near, float far)
		{
			Kind = kind;
			Near = near;
			Far = far;
			if (kind == FrustumKind.Perspective)
			{
				var max = Math.Max (width, height);
				Right = width / max;
				Top = height / max;
			}
			else
			{
				Right = width * 0.5f;
				Top = height * 0.5f;
			}
			Left = -Right;
			Bottom = -Top;
		}
		
		public Mat4 CameraToScreen
		{
			get	
			{ 
				if (!_cameraToScreen.HasValue)
					_cameraToScreen = Kind == FrustumKind.Perspective ?
						Mat.PerspectiveProjection (Left, Right, Bottom, Top, Near, Far) :
						Mat.OrthographicProjection (Left, Right, Bottom, Top, Near, Far);
				return _cameraToScreen.Value; 
			}
		}

		public Vec3[] Corners
		{
			get
			{
				if (_corners == null)
				{
					float backLeft, backRight, backBottom, backTop;
					if (Kind == FrustumKind.Perspective)
					{
						backLeft = (Left / Near) * Far;
						backRight = (Right / Near) * Far;
						backBottom = (Bottom / Near) * Far;
						backTop = (Top / Near) * Far;
					}
					else
					{
						backLeft = Left;
						backRight = Right;
						backBottom = Bottom;
						backTop = Top;
					}
					_corners = new Vec3[8]
					{
						new Vec3 (Left, Bottom, -Near),
						new Vec3 (Left, Top, -Near),
						new Vec3 (Right, Top, -Near),
						new Vec3 (Right, Bottom, -Near),
						new Vec3 (backLeft, backBottom, -Far),
						new Vec3 (backLeft, backTop, -Far),
						new Vec3 (backRight, backTop, -Far),
						new Vec3 (backRight, backBottom, -Far)
					};
				}
				return _corners;
			}
		}
		
		public Plane[] CullingPlanes (Mat4 worldToCamera)
		{
			var corners = Corners.Map (p => worldToCamera.Transform (p));
			return new Plane[6]
			{
				new Plane (corners[1], corners[0], corners[4]),	// left	
				new Plane (corners[3], corners[2], corners[6]),	// right
				new Plane (corners[0], corners[3], corners[7]),	// bottom
				new Plane (corners[2], corners[1], corners[5]),	// top
				new Plane (corners[0], corners[1], corners[2]),	// near
				new Plane (corners[6], corners[5], corners[4])	// far
			};
		}
		
		public static ViewingFrustum FromBBox (BBox bbox)
		{
			return new ViewingFrustum (FrustumKind.Orthographic, bbox.Left, bbox.Right, bbox.Bottom, bbox.Top,
				1f, bbox.Size.Z);
		}
	}
}
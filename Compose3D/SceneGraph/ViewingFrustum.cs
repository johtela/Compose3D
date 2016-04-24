namespace Compose3D.SceneGraph
{
	using System;
	using System.Linq;
	using DataStructures;
	using Maths;
	using Extensions;

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

		public Vec4 XYPlaneAtZ (float Z)
		{
			return Kind == FrustumKind.Perspective ?
				new Vec4 ((Left / Near) * Z, (Bottom / Near) * Z, (Right / Near) * Z, (Top / Near) * Z) :
				new Vec4 (Left, Bottom, Right, Top);
		}

		public Vec3[] Corners
		{
			get
			{
				if (_corners == null)
				{
					var backPlane = XYPlaneAtZ (Far);
					_corners = new Vec3[8]
					{
						new Vec3 (Left, Bottom, -Near),
						new Vec3 (Left, Top, -Near),
						new Vec3 (Right, Top, -Near),
						new Vec3 (Right, Bottom, -Near),
						new Vec3 (backPlane.X, backPlane.Y, -Far),
						new Vec3 (backPlane.X, backPlane.W, -Far),
						new Vec3 (backPlane.Z, backPlane.W, -Far),
						new Vec3 (backPlane.Z, backPlane.Y, -Far)
					};
				}
				return _corners;
			}
		}
		
		public Plane[] CullingPlanes (Mat4 cameraToWorld)
		{
			var corners = Corners.Map (p => cameraToWorld.Transform (p));
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

		public static ViewingFrustum FromBBox (Aabb<Vec3> bbox)
		{
			return new ViewingFrustum (FrustumKind.Orthographic, bbox.Left, bbox.Right, bbox.Bottom, bbox.Top,
				-bbox.Front, -bbox.Back);
		}
	}
}
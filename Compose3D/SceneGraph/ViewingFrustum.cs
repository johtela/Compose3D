namespace Compose3D.SceneGraph
{
	using System;
	using Maths;

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

		private Mat4? _transform;
		private Vec3 [] _corners;

		public ViewingFrustum (FrustumKind kind, float left, float right, float bottom, float top, float near, float far)
		{
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
				Right = width / max * 0.5f;
				Top = height / max * 0.5f;
			}
			else
			{
				Right = width * 0.5f;
				Top = height * 0.5f;
			}
			Left = -Right;
			Bottom = -Top;
		}
		
		public Mat4 Transform
		{
			get	
			{ 
				if (!_transform.HasValue)
					_transform = Kind == FrustumKind.Perspective ?
						Mat.PerspectiveProjection (Left, Right, Bottom, Top, Near, Far) :
						Mat.OrthographicProjection (Left, Right, Bottom, Top, Near, Far);
				return _transform.Value; 
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
						new Vec3 (Left, Bottom, Near),
						new Vec3 (Left, Top, Near),
						new Vec3 (Right, Top, Near),
						new Vec3 (Right, Bottom, Near),
						new Vec3 (backLeft, backBottom, Far),
						new Vec3 (backLeft, backTop, Far),
						new Vec3 (backRight, backTop, Far),
						new Vec3 (backRight, backBottom, Far)
					};
				}
				return _corners;
			}
		}
		
		public Plane[] CullingPlanes (Mat4 viewMatrix)
		{
			var corners = Corners.Map (p => viewMatrix.Transform (p));
			return new Plane[6]
			{
				new Plane (corners[0], corners[1], corners[4]),	
				new Plane (corners[3], corners[2], corners[7]),	
				new Plane (corners[0], corners[3], corners[4]),	
				new Plane (corners[1], corners[2], corners[5]),	
				new Plane (corners[0], corners[1], corners[2]),	
				new Plane (corners[4], corners[5], corners[6])	
			};
		}
		
		public ViewingFrustum IncludePoint (Vec3 point)
		{
			if (Kind != FrustumKind.Orthographic)
				throw new InvalidOperationException ("This method works only for orthographic frustums");
			var min = point.Min (new Vec3 (Left, Bottom, Near));
			var max = point.Max (new Vec3 (Right, Top, Far));
			return new ViewingFrustum (Kind, min.X, max.X, min.Y, max.Y, min.Z, max.Z);
		}
	}
}
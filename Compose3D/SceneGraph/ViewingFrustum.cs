namespace Compose3D.SceneGraph
{
	using System;
	using Maths;

	public enum FrustumKind	{ Perspective, Orthographic }

	public struct ViewingFrustum
	{
		public readonly FrustumKind Kind;
		public readonly float Left;
		public readonly float Right;
		public readonly float Bottom;
		public readonly float Top;
		public readonly float Near;
		public readonly float Far;
		public readonly Mat4 Transform;

		private Vec3 [] _corners;

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
			Transform = kind == FrustumKind.Perspective ?
				Mat.PerspectiveProjection (Left, Right, Bottom, Top, Near, Far) :
				Mat.OrthographicProjection (Left, Right, Bottom, Top, Near, Far);
			_corners = null;
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
							new Vec3 (backLeft, backBottom, Near),
							new Vec3 (backLeft, backTop, Near),
							new Vec3 (backRight, backTop, Near),
							new Vec3 (backRight, backBottom, Near)
						};
				}
				return _corners;
			}
		}
	}
}
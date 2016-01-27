namespace Compose3D.SceneGraph
{
	using System;
	using Maths;
	
	public struct ViewingFrustum
	{
		public readonly float HalfWidth;
		public readonly float HalfHeight;
		public readonly float Near;
		public readonly float Far;
		
		private Vec3 [] _corners;

		public ViewingFrustum (float width, float height, float near, float far)
		{
			var max = Math.Max (width, height);
			HalfWidth = width / max * 0.5f;
			HalfHeight = height / max * 0.5f;
			Near = near;
			Far = far;
			_corners = null;
		}
		
		public Vec3[] Corners
		{
			get
			{
				if (_corners == null)
				{
					var backHW = (HalfWidth / Near) * Far;
					var backHH = (HalfHeight / Near) * Far;
					_corners = new Vec3[8]
						{
							new Vec3 (-HalfWidth, -HalfHeight, Near),
							new Vec3 (-HalfWidth, HalfHeight, Near),
							new Vec3 (HalfWidth, HalfHeight, Near),
							new Vec3 (HalfWidth, -HalfHeight, Near),
							new Vec3 (-backHW, -backHH, Far),
							new Vec3 (-backHW, backHH, Far),
							new Vec3 (backHW, backHH, Far),
							new Vec3 (backHW, -backHH, Far)
						};
				}
				return _corners;
			}
		}
	}
}

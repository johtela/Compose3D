namespace Compose3D.SceneGraph
{
	public struct ViewFrustrum
	{
		public readonly float Left;
		public readonly float Right;
		public readonly float Bottom;
		public readonly float Top;
		public readonly float Near;
		public readonly float Far;

		public ViewFrustrum (float left, float right, float bottom, float top, float near, float far)
		{
			Left = left;
			Right = right;
			Bottom = bottom;
			Top = top;
			Near = near;
			Far = far;
		}
	}
}

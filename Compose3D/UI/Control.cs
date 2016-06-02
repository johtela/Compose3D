namespace Compose3D.UI
{
	using System.Drawing;
	using OpenTK.Input;
	using Visuals;

	public abstract class Control : IVisualizable
	{
		public abstract Visual ToVisual ();

		public abstract void HandleInput (MouseDevice mouse, KeyboardDevice keyboard,
			PointF relativeMousePos);
	}
}

namespace Compose3D.UI
{
	using System.Drawing;
	using OpenTK.Input;
	using Visuals;

	public class Static : Control
	{
		public readonly Visual Visual;

		public Static (Visual visual)
		{
			Visual = visual;
		}

		public override Visual ToVisual ()
		{
			return Visual;
		}

		public override void HandleInput (PointF relativeMousePos)
		{ }

		public static Static Label (string caption, FontStyle style)
		{
			return new Static (Visual.Styled (Visual.Label (caption),
				new VisualStyle (VisualStyle.Default, 
					new Font (VisualStyle.Default.Font, style))));
		}
	}
}

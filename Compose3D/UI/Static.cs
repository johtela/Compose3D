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
			return new Static (Visual.Styled (Visual.Margin (Visual.Label (caption), left: 2f, right: 2f),
				new VisualStyle (VisualStyle.Default, 
					new Font (VisualStyle.Default.Font, style))));
		}
	}
}

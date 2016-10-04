namespace Compose3D.UI
{
	using System.Drawing;
	using System;
	using Visuals;

	public class Label : Control
	{
		public readonly Visual Visual;

		public Label (Visual visual)
		{
			Visual = visual;
		}

		public override Visual ToVisual ()
		{
			return Visual;
		}

		public override void HandleInput (PointF relativeMousePos)
		{ }

		public static Label Static (string caption, FontStyle style)
		{
			return new Label (Visual.Styled (Visual.Margin (Visual.Label (caption), left: 2f, right: 2f),
				new VisualStyle (VisualStyle.Default, 
					new Font (VisualStyle.Default.Font, style))));
		}

		public static Label Dynamic (Func<string> getCaption, FontStyle style)
		{
			return new Label (Visual.Styled (
				Visual.Margin (
					Visual.Delayed (() => Visual.Label (getCaption ())), 
					left: 2f, right: 2f),
				new VisualStyle (VisualStyle.Default,
					new Font (VisualStyle.Default.Font, style))));
		}
	}
}

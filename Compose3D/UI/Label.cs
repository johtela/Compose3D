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

		public static Label Static (string caption, FontStyle fontStyle)
		{
			return new Label (Visual.Styled (Visual.Margin (Visual.Label (caption), left: 2f, right: 2f),
				Style.WithFontStyle (fontStyle)));
		}

		public static Label Dynamic (Func<string> getCaption, FontStyle fontStyle)
		{
			return new Label (Visual.Styled (
				Visual.Margin (
					Visual.Delayed (() => Visual.Label (getCaption ())), 
					left: 2f, right: 2f),
				Style.WithFontStyle (fontStyle)));
		}

		public static Label ColorPreview (Func<Color> getColor, SizeF size)
		{
			return new Label (Visual.Delayed (() =>
				Visual.Styled (
					Visual.Frame (Visual.Empty (new VBox (size)), FrameKind.Rectangle, true),
					new VisualStyle (parent: Style, brush: new SolidBrush (getColor ())))));
		}
	}
}

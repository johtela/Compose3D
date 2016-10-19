namespace Compose3D.UI
{
	using System.Drawing;
	using OpenTK.Input;
	using Visuals;
	using Reactive;

	public class PanelFrame : Control
	{
		public static Color FrameColor = Color.FromArgb (150, 255, 150, 100);
		public static VisualStyle FrameStyle =
			new VisualStyle (
				VisualStyle.Default,
				pen: new Pen (FrameColor, 3f),
				textBrush: Brushes.White,
				brush: new SolidBrush (FrameColor));

		public readonly string Title;
		public readonly Control Client;

		public PanelFrame (Control client, string title)
		{
			Client = client;
			Title = title;
		}
		
		public override void HandleInput (PointF relativeMousePos)
		{
			Client.HandleInput (relativeMousePos);
		}

		public override Visual ToVisual (SizeF panelSize)
		{
			var borderWidth = FrameStyle.Pen.Width;
			var titleBar = Visual.Frame (
				Visual.Margin (Visual.Label (Title), 2f),
				FrameKind.Rectangle, true);
			var psize = new SizeF (panelSize.Width - (2f * borderWidth),
				panelSize.Height - (2f * borderWidth) - FrameStyle.Font.Height - 10f);
			return Visual.Styled (
				Visual.VStack (HAlign.Left, titleBar,
					Visual.Margin (
						Visual.Frame (
							Visual.Styled (Client.ToVisual (psize), Control.Style),
							FrameKind.Rectangle, false),
						borderWidth)),
				FrameStyle);
		}
	}
}

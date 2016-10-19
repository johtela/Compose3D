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
		public readonly Reaction<PointF> Moved;

		private RectangleF _titleRect;
		private RectangleF _clientRect;
		private bool _moving;
		private SizeF _lastPos;

		public PanelFrame (Control client, string title, Reaction<PointF> moved)
		{
			Client = client;
			Title = title;
			Moved = moved;
		}
		
		public override void HandleInput (PointF relativeMousePos)
		{
			if (!_moving && Moved != null && 
				MouseButtonPressed (MouseButton.Left) && 
				_titleRect.Contains (relativeMousePos))
			{
				_moving = true;
				_lastPos = new SizeF (relativeMousePos);
			}
			else if (_moving && MouseButtonDown (MouseButton.Left))
			{
				var delta = relativeMousePos - _lastPos;
				if (delta != PointF.Empty)
				{
					Moved (delta);
					_lastPos = new SizeF (relativeMousePos);
				}
			}
			else
			{
				_moving = false;
				Client.HandleInput (relativeMousePos);
			}
		}

		public override Visual ToVisual (SizeF panelSize)
		{
			var borderWidth = FrameStyle.Pen.Width;
			var titleBar = Visual.Clickable (
				Visual.Frame (
					Visual.Margin (Visual.Label (Title), 2f),
					FrameKind.Rectangle, true),
				rect => _titleRect = rect);
			var psize = new SizeF (panelSize.Width - (2f * borderWidth),
				panelSize.Height - (2f * borderWidth) - FrameStyle.Font.Height - 10f);
			return Visual.Styled (
				Visual.VStack (HAlign.Left, titleBar,
					Visual.Clickable (
						Visual.Margin (
							Visual.Frame (
								Visual.Styled (Client.ToVisual (psize), Control.Style),
								FrameKind.Rectangle, false),
							borderWidth),
						rect => _clientRect = rect)),
				FrameStyle);
		}
	}
}

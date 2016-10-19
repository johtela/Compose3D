namespace Compose3D.UI
{
	using System.Drawing;
	using OpenTK.Input;
	using Reactive;
	using Visuals;

	public class Button : Control
	{
		public readonly string Caption;
		public readonly Reaction<bool> Pressed;

		// Click region
		private RectangleF _clickRegion;
		private bool _pressed;
		private bool _onButton;

		public Button (string caption, Reaction<bool> pressed)
		{
			Caption = caption;
			Pressed = pressed;
		}

		public override void HandleInput (PointF relativeMousePos)
		{
			_onButton = _clickRegion.Contains (relativeMousePos);
			if (InputState.MouseButtonPressed (MouseButton.Left) && _onButton)
				_pressed = true;
			else if (_pressed && !InputState.MouseButtonDown (MouseButton.Left))
			{
				_pressed = false;
				if (_onButton)
					Pressed (true);
			}
		}

		public override Visual ToVisual (SizeF panelSize)
		{
			var highlighted = _pressed && _onButton;
			var visual = Visual.Clickable (
				Visual.Frame (
					Visual.Margin (Visual.Label (Caption), 4f), 
					FrameKind.RoundRectangle, highlighted),
				rect => _clickRegion = rect);
			return highlighted ?
				Visual.Styled (visual, SelectedStyle) :
				visual;
		}
	}
}

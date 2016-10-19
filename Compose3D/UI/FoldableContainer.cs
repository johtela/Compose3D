namespace Compose3D.UI
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Linq;
	using OpenTK.Input;
	using Maths;
	using Imaging;
	using Reactive;
	using Visuals;
	using Extensions;

	public class FoldableContainer : Control
	{
		public readonly Visual Header;
		public readonly Control[] Controls;
		public readonly bool Framed;
		public readonly HAlign Alignment;

		// State
		private bool _folded;
		private RectangleF _clickRegion;

		public FoldableContainer (Visual header, bool framed, HAlign alignment, params Control[] controls)
		{
			Header = header;
			Controls = controls;
			Framed = framed;
			Alignment = alignment;
		}

		public override Visual ToVisual (SizeF panelSize)
		{
			var header = Visual.Clickable (
				Visual.HStack (VAlign.Top,
					Visual.Styled (
						Visual.Label (_folded ? "↓" : "↑"),
						Style.WithFontStyle (FontStyle.Bold)),
					Header), 
				rect => _clickRegion = rect);
			var cvisuals = 
				(_folded ? 
					Enumerable.Empty<Visual> () :
					Controls.Select (c => c.ToVisual (panelSize)))
				.Prepend (header);
			var visual = Visual.Margin (
				Visual.VStack (HAlign.Left, cvisuals.Select (v => Visual.Margin (v, top: 2f, bottom: 2f))),
				2f);
			return Framed ? Visual.Frame (visual, FrameKind.RoundRectangle, true) : visual;
		}

		public override void HandleInput (PointF relativeMousePos)
		{
			if (InputState.MouseButtonPressed (MouseButton.Left) && _clickRegion.Contains (relativeMousePos))
				_folded = !_folded;
			else if (!_folded)
				foreach (var control in Controls)
					control.HandleInput (relativeMousePos);
		}

		public static FoldableContainer WithLabel (string label, bool framed, HAlign alignment, 
			params Control[] controls)
		{
			var header = Visual.Styled (
				Visual.Margin (
					Visual.Label (label),
					left: 2f, right: 8f, top: 2f, bottom: 2f),
				Style.WithFontStyle (FontStyle.Bold));
			return new FoldableContainer (header, framed, alignment, controls);
		}
	}
}
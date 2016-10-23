namespace Compose3D.UI
{
	using System;
	using System.Drawing;
	using Visuals;

	public class Connected : Control
	{
		public readonly Control Source;
		public readonly HAlign HorizAlign;
		public readonly VAlign VertAlign;

		internal Visual _anchor;

		public Connected (Control source, HAlign horizAlign, VAlign vertAlign)
		{
			Source = source;
			HorizAlign = horizAlign;
			VertAlign = vertAlign;
		}

		public override void HandleInput (PointF relativeMousePos)
		{
			Source.HandleInput (relativeMousePos);
		}

		public override Visual ToVisual (SizeF panelSize)
		{
			_anchor = Visual.Anchor (Source.ToVisual (panelSize), HorizAlign, VertAlign);
			return _anchor;
		}
	}
}


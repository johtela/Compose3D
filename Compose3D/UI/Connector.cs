namespace Compose3D.UI
{
	using System;
	using System.Drawing;
	using Visuals;

	public class Connector : Connected
	{
		public readonly Connected Target;

		public Connector (Control source, Connected target, HAlign horizAlign, VAlign vertAlign)
			: base (source, horizAlign, vertAlign)
		{
			Target = target;
		}

		public override Visual ToVisual (SizeF panelSize)
		{
			return Visual.Connector (Source.ToVisual (panelSize), Target._anchor, HorizAlign, VertAlign);
		}
	}
}

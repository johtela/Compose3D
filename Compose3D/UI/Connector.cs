namespace Compose3D.UI
{
	using System;
	using System.Drawing;
	using Visuals;

	public class Connector : Connected
	{
		public readonly Connected Target;
		public readonly VisualDirection Direction;
		public readonly ConnectorKind Kind;
		public readonly VisualStyle ConnectorStyle;

		public Connector (Control source, Connected target, VisualDirection direction, 
			HAlign horizAlign, VAlign vertAlign, ConnectorKind kind, VisualStyle style) 
			: base (source, horizAlign, vertAlign)
		{
			Target = target;
			Kind = kind;
			ConnectorStyle = style;
		}

		public override Visual ToVisual (SizeF panelSize)
		{
			return Visual.Styled (Visual.Connector (Source.ToVisual (panelSize), Target._anchor, Direction, 
				HorizAlign, VertAlign, Kind), ConnectorStyle);
		}
	}
}

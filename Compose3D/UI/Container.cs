namespace Compose3D.UI
{
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using OpenTK.Input;
	using Visuals;

	public class Container : Control
	{
		public readonly VisualDirection Direction;
		public readonly HAlign HorizAlign;
		public readonly VAlign VertAlign;
		public readonly bool Framed;
		public readonly bool WrapAround;
		public readonly List<Control> Controls;

		public Container (VisualDirection direction, HAlign horizAlign,	VAlign vertAlign,  
			bool framed, bool wrapAround, params Control[] controls)
		{
			Direction = direction;
			HorizAlign = horizAlign;
			VertAlign = vertAlign;
			Framed = framed;
			WrapAround = wrapAround;
			Controls = new List<Control> (controls);
		}

		public override Visual ToVisual ()
		{
			var cvisuals = Controls.Select (c => c.ToVisual ());
			var visual = Visual.Margin (
				Direction == VisualDirection.Horizontal ?
					(WrapAround ?
						Visual.HFlow (VertAlign, cvisuals.Select (v => Visual.Margin (v, left: 2f, right: 2f))) :
						Visual.HStack (VertAlign, cvisuals.Select (v => Visual.Margin (v, left: 2f, right: 2f)))) :
					(WrapAround ?
						Visual.VFlow (HorizAlign, cvisuals.Select (v => Visual.Margin (v, top: 2f, bottom: 2f))) :
						Visual.VStack (HorizAlign, cvisuals.Select (v => Visual.Margin (v, top: 2f, bottom: 2f)))),
				2f);
			return Framed ? Visual.Frame (visual, FrameKind.RoundRectangle, true) : visual;
		}

		public override void HandleInput (PointF relativeMousePos)
		{
			foreach (var control in Controls)
				control.HandleInput (relativeMousePos);
		}

		public static Container LabelAndControl (string label, Control control, bool framed)
		{
			return new Container (VisualDirection.Horizontal, HAlign.Left, VAlign.Center, framed, false,
				Label.Static (label, FontStyle.Regular), control);
		}
	}
}

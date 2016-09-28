namespace Compose3D.UI
{
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using OpenTK.Input;
	using Visuals;

	public class Container : Control
	{
		public readonly IEnumerable<Control> Controls;
		public readonly VisualDirection Direction;
		public readonly HAlign HorizAlign;
		public readonly VAlign VertAlign;

		public Container (VisualDirection direction, HAlign horizAlign,
			VAlign vertAlign, IEnumerable<Control> controls)
		{
			Direction = direction;
			Controls = controls;
			HorizAlign = horizAlign;
			VertAlign = vertAlign;
		}

		public Container (VisualDirection direction, HAlign horizAlign,
			VAlign vertAlign, params Control[] controls)
			: this (direction, horizAlign, vertAlign, (IEnumerable<Control>)controls)
		{ }

		public override Visual ToVisual ()
		{
			var visuals = Controls.Select (c => c.ToVisual ());
			return Direction == VisualDirection.Horizontal ?
				Visual.HStack (VertAlign, visuals) :
				Visual.VStack (HorizAlign, visuals);
		}

		public override void HandleInput (PointF relativeMousePos)
		{
			foreach (var control in Controls)
				control.HandleInput (relativeMousePos);
		}
	}
}
